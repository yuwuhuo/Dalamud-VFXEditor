using Dalamud.Interface;
using ImGuiNET;
using System.IO;
using VfxEditor.Utils;

namespace VfxEditor.AvfxFormat.Vfx {
    public interface IUiNodeView<T> where T : UiNode {
        public void RemoveFromAvfx( T item );
        public void AddToAvfx( T item, int idx );

        public T AddToAvfxAndGroup( BinaryReader reader, long position, int size, string renamed, bool hasDependencies ) {
            reader.BaseStream.Seek( position, SeekOrigin.Begin );
            var newItem = AddToAvfx( reader, size, hasDependencies );
            if( !string.IsNullOrEmpty( renamed ) ) newItem.Renamed = renamed;
            AddToGroup( newItem );
            return newItem;
        }
        public T AddToAvfx( BinaryReader reader, int size, bool hasDependencies );

        public void ImportDefault();

        public void AddToGroup( T item );
        public void ResetSelected();

        public static void DrawControls( IUiNodeView<T> nodeView, AvfxFile vfxFile, T selected, UiNodeGroup<T> group, bool allowNew, bool allowDelete, string Id ) {
            ImGui.PushFont( UiBuilder.IconFont );
            if( allowNew ) {
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Plus}" + Id ) ) {
                    ImGui.OpenPopup( "New_Popup" + Id );
                }
            }
            if( selected != null && allowDelete ) {
                ImGui.SameLine();
                ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 4 );
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Save}" + Id ) ) {
                    vfxFile.ShowExportDialog( selected );
                }

                ImGui.SameLine();
                ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 4 );
                if( ImGui.Button( $"{( char )FontAwesomeIcon.BookMedical}" + Id ) ) {
                    vfxFile.AddToNodeLibrary( selected );
                }
                // Tooltip
                ImGui.PopFont();
                UiUtils.Tooltip( "Add to node library" );
                ImGui.PushFont( UiBuilder.IconFont );

                ImGui.SameLine();
                ImGui.SetCursorPosX( ImGui.GetCursorPosX() + 20 );
                if( UiUtils.RemoveButton( $"{( char )FontAwesomeIcon.Trash}" + Id ) ) CommandManager.Avfx.Add( new UiNodeViewRemoveCommand<T>( nodeView, group, selected ) );
            }
            ImGui.PopFont();

            // ===== NEW =====
            if( ImGui.BeginPopup( "New_Popup" + Id ) ) {
                if( ImGui.Selectable( "Default" + Id ) ) {
                    nodeView.ImportDefault();
                }
                if( ImGui.Selectable( "Import" + Id ) ) vfxFile.ShowImportDialog();
                if( selected != null && ImGui.Selectable( "Duplicate" + Id ) ) {
                    using var ms = new MemoryStream();
                    using var writer = new BinaryWriter( ms );
                    using var reader = new BinaryReader( ms );

                    selected.Write( writer );
                    reader.BaseStream.Seek( 0, SeekOrigin.Begin );
                    reader.ReadInt32(); // Name
                    var size = reader.ReadInt32();
                    var newNode = nodeView.AddToAvfx( reader, size, false );
                    newNode.Renamed = selected.Renamed;
                    group.AddAndUpdate( newNode );
                }
                ImGui.EndPopup();
            }
        }
    }
}
