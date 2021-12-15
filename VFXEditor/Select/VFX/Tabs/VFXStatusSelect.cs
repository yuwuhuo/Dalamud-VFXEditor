using System.Numerics;
using ImGuiNET;
using VFXSelect.Select.Rows;

namespace VFXSelect.VFX {
    public class VFXStatusSelect : VFXSelectTab<XivStatus, XivStatus> {
        private ImGuiScene.TextureWrap Icon;

        public VFXStatusSelect( string parentId, string tabId, VFXSelectDialog dialog ) :
            base( parentId, tabId, SheetManager.Statuses, dialog ) {
        }

        protected override bool CheckMatch( XivStatus item, string searchInput ) {
            return Matches( item.Name, searchInput );
        }

        protected override void OnSelect() {
            LoadIcon( Selected.Icon, ref Icon );
        }

        protected override void DrawSelected( XivStatus loadedItem ) {
            if( loadedItem == null ) { return; }
            ImGui.Text( loadedItem.Name );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( Icon != null ) {
                ImGui.Image( Icon.ImGuiHandle, new Vector2( Icon.Width, Icon.Height ) );
            }

            // ==== LOOP 1 =====
            ImGui.Text( "Loop VFX1: " );
            ImGui.SameLine();
            DisplayPath( loadedItem.GetLoopVFX1Path() );
            if( loadedItem.LoopVFX1Exists ) {
                if( ImGui.Button( "SELECT" + Id + "Loop1" ) ) {
                    Dialog.Invoke( new SelectResult( SelectResultType.GameStatus, "[STATUS] " + loadedItem.Name + " Loop1", loadedItem.GetLoopVFX1Path() ) );
                }
                ImGui.SameLine();
                Copy( loadedItem.GetLoopVFX1Path(), id: Id + "Loop1Copy" );
                Dialog.Spawn( loadedItem.GetLoopVFX1Path(), id: Id + "Loop1Spawn" );
            }

            // ==== LOOP 2 =====
            ImGui.Text( "Loop VFX2: " );
            ImGui.SameLine();
            DisplayPath( loadedItem.GetLoopVFX2Path() );
            if( loadedItem.LoopVFX2Exists ) {
                if( ImGui.Button( "SELECT" + Id + "Loop2" ) ) {
                    Dialog.Invoke( new SelectResult( SelectResultType.GameStatus, "[STATUS] " + loadedItem.Name + " Loop2", loadedItem.GetLoopVFX2Path() ) );
                }
                ImGui.SameLine();
                Copy( loadedItem.GetLoopVFX2Path(), id: Id + "Loop2Copy" );
                ImGui.SameLine();
                Dialog.Spawn( loadedItem.GetLoopVFX2Path(), id: Id + "Loop2Spawn" );
            }

            // ==== LOOP 3 =====
            ImGui.Text( "Loop VFX3: " );
            ImGui.SameLine();
            DisplayPath( loadedItem.GetLoopVFX3Path() );
            if( loadedItem.LoopVFX3Exists ) {
                if( ImGui.Button( "SELECT" + Id + "Loop3" ) ) {
                    Dialog.Invoke( new SelectResult( SelectResultType.GameStatus, "[STATUS] " + loadedItem.Name + " Loop3", loadedItem.GetLoopVFX3Path() ) );
                }
                ImGui.SameLine();
                Copy( loadedItem.GetLoopVFX3Path(), id: Id + "Loop3Copy" );
                Dialog.Spawn( loadedItem.GetLoopVFX3Path(), id: Id + "Loop3Spawn" );
            }
        }

        protected override string UniqueRowTitle( XivStatus item ) {
            return item.Name + "##" + item.RowId;
        }
    }
}