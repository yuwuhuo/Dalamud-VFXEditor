using Dalamud.Interface;
using ImGuiFileDialog;
using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.Numerics;
using VfxEditor.Library.Texture;
using VfxEditor.Utils;

namespace VfxEditor.Library.Node {
    public class TextureRoot : LibraryFolder {
        private string AddPath = "";

        public TextureRoot( List<LibraryProps> items ) : base( null, "", "", items ) { }

        public override unsafe bool Draw( LibraryManager library, string searchInput ) {
            ImGui.InputTextWithHint( "##Add", "vfx/action/some_texture.atex", ref AddPath, 256 );
            using( var style = ImRaii.PushStyle( ImGuiStyleVar.ItemSpacing, new Vector2( 4, 4 ) ) )
            using( var font = ImRaii.PushFont( UiBuilder.IconFont ) ) {
                ImGui.SameLine();
                if( ImGui.Button( FontAwesomeIcon.Plus.ToIconString() ) ) {
                    AddTexture( AddPath, AddPath );
                    library.Save();
                    AddPath = "";
                }

                ImGui.SameLine();
                if( ImGui.Button( FontAwesomeIcon.Save.ToIconString() ) ) {
                    FileDialogManager.SaveFileDialog( "Select a Save Location", ".txt", "Textures", "txt", ( bool ok, string res ) => {
                        if( !ok ) return;
                        library.ExportTextures( res );
                    } );
                }

                ImGui.SameLine();
                if( ImGui.Button( FontAwesomeIcon.FileImport.ToIconString() ) ) {
                    FileDialogManager.OpenFileDialog( "Select a File", ".txt,.*", ( bool ok, string res ) => {
                        if( !ok ) return;
                        library.ImportTextures( res );
                    } );
                }
            }

            ImGui.Separator();

            using var child = ImRaii.Child( "Child", ImGui.GetContentRegionAvail(), false );

            if( Children.Count == 0 ) ImGui.TextDisabled( "No textures saved..." );
            return base.Draw( library, searchInput );
        }

        public unsafe void AddTexture( string name, string path ) {
            if( string.IsNullOrEmpty( path ) || !path.Contains( '/' ) ) return;
            Children.Add( new TextureLeaf( this, name, UiUtils.RandomString( 12 ), path, *ImGui.GetStyleColorVec4( ImGuiCol.Header ) ) );
        }
    }
}
