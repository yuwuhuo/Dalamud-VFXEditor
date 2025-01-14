using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Raii;
using System;
using System.IO;
using System.Numerics;
using VfxEditor.FileManager;
using VfxEditor.Parsing;
using VfxEditor.TmbFormat.Root;
using VfxEditor.Utils;

namespace VfxEditor.TmbFormat {
    public enum LuaOperation {
        None = 0x0,
        Open_Parens = 0x1,
        Close_Parens = 0x2,
        Add = 0x3,
        Sub = 0x4,
        Mul = 0x5,
        Div = 0x6,
        Mod = 0x7,
        Greater = 0x8,
        Greater_Eq = 0x9,
        Less = 0xA,
        Less_Eq = 0xB,
        Not_Eq = 0xC,
        Eq = 0xD,
        And = 0xE,
        Or = 0xF,
        Not = 0x10,
        Int_Value = 0x11,
        Float_Value = 0x12,
        Variable = 0x13,
        True = 0x14,
        False = 0x15,
        Abs = 0x16,
        Acos = 0x17,
        Asin = 0x18,
        Atan2 = 0x19,
        Atan = 0x1A,
        Ceil = 0x1B,
        Cosh = 0x1C,
        Cos = 0x1D,
        Deg = 0x1E,
        Exp = 0x1F,
        Floor = 0x20,
        Fmod = 0x21,
        Frexp = 0x22,
        Ldexp = 0x23,
        Log10 = 0x24,
        Log = 0x25,
        Max = 0x26,
        Min = 0x27,
        Modf = 0x28,
        Pow = 0x29,
        Rad = 0x2A,
        Random = 0x2B,
        Random_Seed = 0x2C,
        Sinh = 0x2D,
        Sin = 0x2E,
        Sqrt = 0x2F,
        Tanh = 0x30,
        Tan = 0x31,
    }

    public class TmtrLuaEntry {
        public readonly TmbFile File;
        public readonly Tmtr Track;

        private readonly ParsedEnum<LuaOperation> Operation = new( "Operation" );
        private readonly ParsedUInt Value = new( "Value" );
        private readonly ParsedFloat FloatValue = new( "Float Value" );

        public TmtrLuaEntry( TmbFile file, Tmtr track ) {
            File = file;
            Track = track;
        }

        public TmtrLuaEntry( BinaryReader reader, TmbFile file, Tmtr track ) : this( file, track ) {
            Operation.Read( reader );
            Value.Read( reader );
            FloatValue.Read( reader );
        }

        public void Write( BinaryWriter writer ) {
            Operation.Write( writer );
            Value.Write( writer );
            FloatValue.Write( writer );
        }

        public bool Draw( bool first, float maxX, ref float currentX ) {
            var text = Operation.Value switch {
                LuaOperation.Int_Value => $"{Value.Value}",
                LuaOperation.Float_Value => $"{FloatValue.Value}",
                LuaOperation.Variable => GetVariableText( VariablePool, VariableIndex ),
                LuaOperation.Open_Parens => new string( '(', ( int )Math.Clamp( Value.Value, 0, 99 ) ),
                LuaOperation.Close_Parens => new string( ')', ( int )Math.Clamp( Value.Value, 0, 99 ) ),
                _ => $"{Operation.Value}"
            };

            var color = Operation.Value switch {
                LuaOperation.Int_Value => Plugin.Configuration.LuaLiteralColor,
                LuaOperation.Float_Value => Plugin.Configuration.LuaLiteralColor,
                LuaOperation.True => Plugin.Configuration.LuaLiteralColor,
                LuaOperation.False => Plugin.Configuration.LuaLiteralColor,
                LuaOperation.Variable => Plugin.Configuration.LuaVariableColor,
                LuaOperation.Open_Parens => Plugin.Configuration.LuaParensColor,
                LuaOperation.Close_Parens => Plugin.Configuration.LuaParensColor,
                _ => Plugin.Configuration.LuaFunctionColor
            };


            using( var padding = ImRaii.PushStyle( ImGuiStyleVar.FramePadding, new Vector2( 6, 2 ) ) )
            using( var font = ImRaii.PushFont( UiBuilder.MonoFont ) ) {
                var imguiStyle = ImGui.GetStyle();
                var width = ImGui.CalcTextSize( text ).X + imguiStyle.FramePadding.X * 2 + 6;

                if( first ) {
                    currentX += width;
                }
                else {
                    if( ( maxX - currentX - width ) > 10 ) {
                        currentX += width;
                        using var spacing = ImRaii.PushStyle( ImGuiStyleVar.ItemSpacing, new Vector2( 6, 6 ) );
                        ImGui.SameLine();
                    }
                    else {
                        currentX = width; // New line
                    }
                }

                using var _ = ImRaii.PushColor( ImGuiCol.Button, color );
                if( ImGui.Button( text ) ) ImGui.OpenPopup( "LuaPopup" );
            }

            using var popup = ImRaii.Popup( "LuaPopup" );

            if( !popup ) return false;

            return DrawPopup();
        }

        private bool DrawPopup() {
            using( var font = ImRaii.PushFont( UiBuilder.IconFont ) ) {
                if( UiUtils.RemoveButton( FontAwesomeIcon.Trash.ToIconString() ) ) {
                    File.Command.Add( new GenericRemoveCommand<TmtrLuaEntry>( Track.LuaEntries, this ) );
                    ImGui.CloseCurrentPopup();
                    return true;
                }
            }

            Operation.Draw( File.Command );

            if( Operation.Value == LuaOperation.Variable ) DrawVariablePopup();
            else {
                Value.Draw( File.Command );
                FloatValue.Draw( File.Command );
            }

            return false;
        }

        private void DrawVariablePopup() {
            var pool = VariablePool;
            var value = VariableIndex;

            var update = false;

            if( ImGui.InputInt( "Pool", ref pool ) ) {
                if( pool < 0 ) pool = 0;
                if( pool > 3 ) pool = 3;
                update = true;
            }
            if( ImGui.InputInt( "Index", ref value ) ) {
                update = true;
            }

            if( update ) {
                var newValue = ( ( uint )pool << 28 ) | ( ( uint )value );
                File.Command.Add( new ParsedSimpleCommand<uint>( Value, newValue ) );
            }
        }

        private int VariablePool => ( int )( Value.Value >> 28 );
        private int VariableIndex => ( int )( Value.Value & 0x0FFFFFFF );

        private static string GetVariableText( int pool, int index ) {
            var luaPool = GetPool( pool );
            if( luaPool == null ) return $"VAR[{pool},{index}]";
            return luaPool.Names.TryGetValue( index, out var name ) ? name : $"VAR[{pool},{index}]";
        }


        private static LuaPool GetPool( int pool ) => pool switch {
            1 => LuaPool.Pool1,
            2 => LuaPool.Pool2,
            3 => LuaPool.Pool3,
            _ => null
        };
    }
}
