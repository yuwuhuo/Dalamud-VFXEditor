using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using VfxEditor.FileManager;
using VfxEditor.Utils;

namespace VfxEditor.UldFormat.PartList {
    public class UldPartList : UldWorkspaceItem {
        public readonly List<UldPartItem> Parts = new();

        private int Offset => 12 + Parts.Count * 12;

        public UldPartList() { }

        public UldPartList( BinaryReader reader ) {
            Id.Read( reader );
            var partCount = reader.ReadInt32();
            reader.ReadInt32(); // skip offset

            for( var i = 0; i < partCount; i++ ) {
                Parts.Add( new UldPartItem( reader ) );
            }
        }

        public void Write( BinaryWriter writer ) {
            Id.Write( writer );
            writer.Write( Parts.Count );
            writer.Write( Offset );
            foreach( var part in Parts ) part.Write( writer );
        }

        public override void Draw() {
            DrawRename();
            Id.Draw( CommandManager.Uld );

            for( var idx = 0; idx < Parts.Count; idx++ ) {
                using var _ = ImRaii.PushId( idx );

                var item = Parts[idx];
                var currentTexture = item.CurrentTexture;
                var text = currentTexture != null ? currentTexture.GetText() : $"Texture {item.TextureId.Value}";

                if( ImGui.CollapsingHeader( $"Part {idx} ({text})###{idx}" ) ) {
                    using var indent = ImRaii.PushIndent();

                    if( UiUtils.RemoveButton( "Delete", true ) ) { // REMOVE
                        CommandManager.Uld.Add( new GenericRemoveCommand<UldPartItem>( Parts, item ) );
                        break;
                    }

                    item.Draw();
                }
            }

            if( ImGui.Button( "+ New" ) ) { // NEW
                CommandManager.Uld.Add( new GenericAddCommand<UldPartItem>( Parts, new UldPartItem() ) );
            }
        }

        public override string GetDefaultText() => $"Part List {GetIdx()}";

        public override string GetWorkspaceId() => $"PartList{GetIdx()}";
    }
}
