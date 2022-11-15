using ImGuiNET;
using System;
using System.IO;
using VfxEditor.Parsing;

namespace VfxEditor.AvfxFormat2 {
    public class AvfxTimelineClip : AvfxWorkspaceItem {
        public readonly AvfxTimeline Timeline;

        public readonly AvfxTimelineClipType Type = new();

        public readonly ParsedInt Unk1 = new( "Raw Int 1" );
        public readonly ParsedInt Unk2 = new( "Raw Int 2" );
        public readonly ParsedInt Unk3 = new( "Raw Int 3" );
        public readonly ParsedInt Unk4 = new( "Raw Int 4" );
        public readonly UiParsedInt4 RawInts;

        public readonly ParsedFloat Unk5 = new( "Raw Float 1" );
        public readonly ParsedFloat Unk6 = new( "Raw Float 2" );
        public readonly ParsedFloat Unk7 = new( "Raw Float 3" );
        public readonly ParsedFloat Unk8 = new( "Raw Float 4" );
        public readonly UiParsedFloat4 RawFloats;

        public AvfxTimelineClip( AvfxTimeline timeline ) : base( "Clip" ) {
            Timeline = timeline;
            Unk5.Value = -1f;

            RawInts = new( "Raw Ints", Unk1, Unk2, Unk3, Unk4 );
            RawFloats = new( "Raw Flaots", Unk5, Unk6, Unk7, Unk8 );
        }

        public override void ReadContents( BinaryReader reader, int size ) {
            Type.Read( reader );
            Unk1.Read( reader, 4 );
            Unk2.Read( reader, 4 );
            Unk3.Read( reader, 4 );
            Unk4.Read( reader, 4 );
            Unk5.Read( reader, 4 );
            Unk6.Read( reader, 4 );
            Unk7.Read( reader, 4 );
            Unk8.Read( reader, 4 );
            reader.ReadBytes( 4 * 32 );
        }

        protected override void RecurseChildrenAssigned( bool assigned ) { }

        protected override void WriteContents( BinaryWriter writer ) {
            Type.Write( writer );
            Unk1.Write( writer );
            Unk2.Write( writer );
            Unk3.Write( writer );
            Unk4.Write( writer );
            Unk5.Write( writer );
            Unk6.Write( writer );
            Unk7.Write( writer );
            Unk8.Write( writer );
            WritePad( writer, 4 * 32 );
        }

        public override void Draw( string parentId ) {
            var id = parentId + "/Clip";
            DrawRename( id );

            Type.Draw( id );
            
            // ====== KILL ============

            if( Type.Value == "LLIK" ) {
                var duration = Unk1.Value;
                if( ImGui.InputInt( "Fade Out Duration" + id, ref duration ) ) {
                    CommandManager.Avfx.Add( new ParsedIntCommand( Unk1, duration ) );
                }

                var hide = Unk4.Value == 1;
                if( ImGui.Checkbox( "Hide" + id, ref hide ) ) {
                    CommandManager.Avfx.Add( new ParsedIntCommand( Unk4, hide ? 1 : 0 ) );
                }

                var allowShow = Unk5.Value != -1f;
                if( ImGui.Checkbox( "Allow Show" + id, ref allowShow ) ) {
                    CommandManager.Avfx.Add( new ParsedFloatCommand( Unk5, allowShow ? 0 : -1f ) );
                }

                var startHidden = Unk6.Value != -1f;
                if( ImGui.Checkbox( "Start Hidden" + id, ref startHidden ) ) {
                    CommandManager.Avfx.Add( new ParsedFloatCommand( Unk6, startHidden ? 0 : -1f ) );
                }
            }

            // ======================

            RawInts.Draw( id, CommandManager.Avfx );
            RawFloats.Draw( id, CommandManager.Avfx );
        }

        public override string GetDefaultText() => $"{GetIdx()}: {Type.Text}";

        public override string GetWorkspaceId() => $"{Timeline.GetWorkspaceId()}/Clip{GetIdx()}";
    }
}
