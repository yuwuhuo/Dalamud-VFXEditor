using ImGuiNET;
using System.Collections.Generic;
using VfxEditor.AVFXLib;
using VfxEditor.Data;
using VfxEditor.Utils;

namespace VfxEditor.AvfxFormat.Vfx {
    public class UiNodeSelectList<T> : UiNodeSelect where T : UiNode {
        public List<T> Selected = new();
        public AVFXIntList Literal;
        public UiNodeGroup<T> Group;
        public string Name;

        public UiNodeSelectList( UiNode node, string name, UiNodeGroup<T> group, AVFXIntList literal ) {
            Node = node;
            Name = name;
            Group = group;
            Literal = literal;
            Group.OnChange += UpdateNode;
            if( Group.IsInitialized ) {
                SetupNode();
            }
            else {
                Group.OnInit += SetupNode;
            }
            node.Selectors.Add( this );
        }

        public override void DrawInline( string parentId ) {
            if( CopyManager.IsCopying ) CopyManager.Copied[Name] = Literal;
            if( CopyManager.IsPasting && CopyManager.Copied.TryGetValue( Name, out var b ) && b is AVFXIntList literal ) {
                Literal.SetValue( literal.GetValue() );
                Literal.SetAssigned( literal.IsAssigned() );

                foreach( var selected in Selected ) UnlinkFrom( selected );
                Selected.Clear();
                foreach( var item in Literal.GetValue() ) {
                    if( item >= 0 && item < Group.Items.Count ) {
                        Selected.Add( Group.Items[item] );
                        LinkTo( Group.Items[item] );
                    }
                    else Selected.Add( null );
                }
            }

            // ====== DRAW =================
            var id = parentId + "/Node";

            if( !Literal.IsAssigned() ) {
                if( ImGui.SmallButton( $"+ {Name}{id}" ) ) Literal.SetAssigned( true );
                return;
            }

            for( var i = 0; i < Selected.Count; i++ ) {
                var _id = id + i;
                var text = ( i == 0 ) ? Name : "";
                if( ImGui.BeginCombo( text + _id, Selected[i] == null ? "[NONE]" : Selected[i].GetText() ) ) {
                    if( ImGui.Selectable( "[NONE]", Selected[i] == null ) ) {
                        UnlinkFrom( Selected[i] );
                        Selected[i] = null;
                        UpdateNode();
                    }
                    foreach( var item in Group.Items ) {
                        if( ImGui.Selectable( item.GetText(), Selected[i] == item ) ) {
                            UnlinkFrom( Selected[i] );
                            LinkTo( item );
                            Selected[i] = item;
                            UpdateNode();
                        }
                        if( ImGui.IsItemHovered() ) {
                            item.ShowTooltip();
                        }
                    }
                    ImGui.EndCombo();
                }

                if( IUiBase.DrawUnassignContextMenu( id, Name ) ) Literal.SetAssigned( false );

                if( i > 0 ) {
                    ImGui.SameLine();
                    if( UiUtils.RemoveButton( "- Remove" + _id, small: true ) ) {
                        UnlinkFrom( Selected[i] );
                        Selected.RemoveAt( i );
                        return;
                    }
                }
            }

            if( Selected.Count == 0 ) {
                ImGui.Text( Name );
                ImGui.TextColored( UiUtils.RED_COLOR, "WARNING: Add an item!" );
            }
            if( Group.Items.Count == 0 ) {
                ImGui.TextColored( UiUtils.RED_COLOR, "WARNING: Add a selectable item first!" );
            }
            if( Selected.Count < 4 ) {
                if( ImGui.SmallButton( "+ " + Name + id ) ) {
                    Selected.Add( Group.Items[0] );
                    LinkTo( Group.Items[0] );
                }

                if( IUiBase.DrawUnassignContextMenu( id, Name ) ) Literal.SetAssigned( false );
            }
        }

        public override void DeleteSelect() {
            UnlinkChange();
            foreach( var item in Selected ) {
                UnlinkFrom( item );
            }
        }

        public override void UnlinkChange() {
            Group.OnChange -= UpdateNode;
        }

        public override void UpdateNode() {
            var idxs = new List<int>();
            foreach( var item in Selected ) {
                if( item == null ) {
                    idxs.Add( 255 );
                }
                else {
                    idxs.Add( item.Idx );
                }
            }
            Literal.SetValue( idxs );
        }

        public override void SetupNode() {
            for( var i = 0; i < Literal.GetValue().Count; i++ ) {
                var val = Literal.GetValue()[i];
                if( Node.HasDependencies && val != 255 && val >= 0 ) {
                    val += Group.PreImportSize;
                    Literal.SetValue( val, i );
                }
                if( val != 255 && val >= 0 && val < Group.Items.Count ) {
                    var item = Group.Items[val];
                    Selected.Add( item );
                    LinkTo( item );
                }
                else {
                    Selected.Add( null );
                }
            }
        }

        public override void DeleteNode( UiNode node ) {
            Selected.RemoveAll( x => x == node );
            UpdateNode();
        }
    }
}