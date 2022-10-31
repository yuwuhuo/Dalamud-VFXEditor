using System.Linq;
using VfxEditor.Select.Rows;

namespace VfxEditor.Select.Sheets {
    public class NonPlayerActionTmbSheetLoader : ActionTmbSheetLoader {
        public override void OnLoad() {
            var sheet = VfxEditor.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()
                .Where( x => !string.IsNullOrEmpty( x.Name ) && !( x.IsPlayerAction || x.ClassJob.Value != null ) && !x.AffectsPosition );
            foreach( var item in sheet ) {
                Items.Add( new XivActionTmb( item ) );
            }
        }

        public override bool SelectItem( XivActionTmb item, out XivActionTmb selectedItem ) {
            selectedItem = item;
            return true;
        }
    }
}