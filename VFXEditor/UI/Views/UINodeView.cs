using AVFXLib.AVFX;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFXEditor.UI.VFX;

namespace VFXEditor.UI.VFX {
    public interface UINodeView<T> where T : UINode {
        public void OnDelete( T item );
        public T OnImport( AVFXNode node, bool has_dependencies = false );

        public void ControlDelete();
        public void ControlCreate();

        public static void DrawControls(UINodeView<T> view, UIMain main, T selected, UINodeGroup<T> group, bool allowNew, bool allowDelete, string Id) {
            ImGui.PushFont( UiBuilder.IconFont );
            if( allowNew ) {
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Plus}" + Id ) ) {
                    ImGui.OpenPopup( "New_Popup" + Id );
                }
            }
            if( selected != null && allowDelete ) {
                ImGui.SameLine();
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Save}" + Id ) ) {
                    ImGui.OpenPopup( "Save_Popup" + Id );
                }
                ImGui.SameLine();
                if( UIUtils.RemoveButton( $"{( char )FontAwesomeIcon.Trash}" + Id ) ) {
                    group.Remove( selected );
                    selected.DeleteNode();
                    view.OnDelete( selected );
                    view.ControlDelete();
                }
            }
            ImGui.PopFont();
            // ===== NEW =====
            if( ImGui.BeginPopup( "New_Popup" + Id ) ) {
                if( ImGui.Selectable( "Create" + Id ) ) {
                    view.ControlCreate();
                }
                if( ImGui.Selectable( "Import" + Id ) ) {
                    main.ImportDialog();
                }
                if( selected != null && ImGui.Selectable( "Duplicate" + Id ) ) {
                    group.Add( view.OnImport( UIMain.CloneNode( selected.ToBytes() ) ) );
                }
                ImGui.EndPopup();
            }
            // ==== SAVE =====
            if( ImGui.BeginPopup( "Save_Popup" + Id ) ) {
                if( ImGui.Selectable( "Export" + Id ) ) {
                    main.ExportMultiple( selected );
                }
                if( ImGui.Selectable( "Simple Export (old)" + Id ) ) {
                    UIMain.ExportDialog( selected );
                }
                ImGui.EndPopup();
            }
        }
    }
}
