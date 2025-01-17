using System.IO;
using System.Linq;
using VfxEditor.Select.Shared.Common;

namespace VfxEditor.Select.Uld.Common {
    public class CommonTab : SelectTab<CommonRow> {
        public CommonTab( SelectDialog dialog, string name ) : base( dialog, name, "Uld-Common" ) { }

        // ===== LOADING =====

        public override void LoadData() {
            var lineIdx = 0;
            foreach( var line in File.ReadLines( SelectUtils.MiscUldPath ).Where( x => !string.IsNullOrEmpty( x ) ) ) {
                Items.Add( new CommonRow( lineIdx, line, line.Replace( ".uld", "" ).Replace( "ui/uld/", "" ), 0 ) );
                lineIdx++;
            }
        }

        // ===== DRAWING ======

        protected override void DrawSelected() {
            Dialog.DrawPath( "Path", Selected.Path, SelectResultType.GameMisc, Selected.Name, true );
        }

        protected override string GetName( CommonRow item ) => item.Name;
    }
}
