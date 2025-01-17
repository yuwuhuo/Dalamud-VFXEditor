using System.Linq;

namespace VfxEditor.Select.Vfx.Status {
    public class StatusTab : SelectTab<StatusRow> {
        public StatusTab( SelectDialog dialog, string name ) : base( dialog, name, "Vfx-Status" ) { }

        // ===== LOADING =====

        public override void LoadData() {
            var sheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>().Where( x => !string.IsNullOrEmpty( x.Name ) );
            foreach( var item in sheet ) {
                var status = new StatusRow( item );
                if( status.VfxExists ) Items.Add( status );
            }
        }

        // ===== DRAWING ======

        protected override void OnSelect() => LoadIcon( Selected.Icon );

        protected override void DrawSelected() {
            SelectTabUtils.DrawIcon( Icon );

            Dialog.DrawPath( "Hit", Selected.HitVfxPath, SelectResultType.GameStatus, $"{Selected.Name} Hit", true );
            Dialog.DrawPath( "Loop 1", Selected.LoopVfxPath1, SelectResultType.GameStatus, $"{Selected.Name} Loop 1", true );
            Dialog.DrawPath( "Loop 2", Selected.LoopVfxPath2, SelectResultType.GameStatus, $"{Selected.Name} Loop 2", true );
            Dialog.DrawPath( "Loop 3", Selected.LoopVfxPath3, SelectResultType.GameStatus, $"{Selected.Name} Loop 3", true );
        }

        protected override string GetName( StatusRow item ) => item.Name;
    }
}
