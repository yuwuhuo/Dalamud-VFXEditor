using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using static VfxEditor.AvfxFormat.Enums;

namespace VfxEditor.AvfxFormat {
    public class AvfxCurve2Axis : AvfxOptional {
        public readonly string Name;
        public readonly bool Locked;

        public readonly AvfxEnum<AxisConnect> AxisConnectType = new( "Axis Connect", "ACT" );
        public readonly AvfxEnum<RandomType> AxisConnectRandomType = new( "Axis Connect Random", "ACTR" );
        public readonly AvfxCurve X;
        public readonly AvfxCurve Y;
        public readonly AvfxCurve RX;
        public readonly AvfxCurve RY;

        private readonly List<AvfxBase> Parsed;
        private readonly List<AvfxCurve> Curves;

        public AvfxCurve2Axis( string name, string avfxName, CurveType type = CurveType.Base, bool locked = false ) : base( avfxName ) {
            Name = name;
            Locked = locked;
            X = new( "X", "X", type );
            Y = new( "Y", "Y", type );
            RX = new( "Random X", "XR", type );
            RY = new( "Random Y", "YR", type );

            Parsed = new() {
                AxisConnectType,
                AxisConnectRandomType,
                X,
                Y,
                RX,
                RY
            };

            Curves = new() {
                X,
                Y,
                RX,
                RY
            };
        }

        public override void ReadContents( BinaryReader reader, int size ) => ReadNested( reader, Parsed, size );

        protected override void RecurseChildrenAssigned( bool assigned ) => RecurseAssigned( Parsed, assigned );

        protected override void WriteContents( BinaryWriter writer ) => WriteNested( writer, Parsed );

        public override void DrawUnassigned() {
            using var _ = ImRaii.PushId( Name );

            AssignedCopyPaste( this, Name );
            DrawAddButtonRecurse( this, Name );
        }

        public override void DrawAssigned() {
            using var _ = ImRaii.PushId( Name );

            AssignedCopyPaste( this, Name );
            if( !Locked && DrawRemoveButton( this, Name ) ) return;

            AvfxCurve.DrawUnassignedCurves( Curves );

            AxisConnectType.Draw();
            AxisConnectRandomType.Draw();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            AvfxCurve.DrawAssignedCurves( Curves );
        }

        public override string GetDefaultText() => Name;
    }
}
