using ImGuiNET;
using System.IO;
using VfxEditor.Interop;
using VfxEditor.Ui.Components;
using VfxEditor.Utils;

namespace VfxEditor.PapFormat {
    public class PapImportModal : Modal {
        private readonly PapFile File;
        private readonly string ImportPath;
        private int Index;

        public PapImportModal( PapFile file, string importPath ) : base( "Animation Import Index" ) {
            File = file;
            ImportPath = importPath;
        }

        protected override void DrawBody() {
            ImGui.PushTextWrapPos( 240 );
            ImGui.TextWrapped( "Select the index of the animation being imported" );
            ImGui.PopTextWrapPos();

            ImGui.InputInt( "##Index", ref Index );
        }

        protected override void OnCancel() { }

        protected override void OnOk() {
            var animation = new PapAnimation( File, File.HkxTempLocation );
            animation.ReadTmb( Path.Combine( Plugin.RootLocation, "Files", "default_pap_tmb.tmb" ), File.Command );

            CompoundCommand command = new( false, true );
            command.Add( new PapAnimationAddCommand( File, File.Animations, animation ) );
            command.Add( new PapHavokFileCommand( animation, File.HkxTempLocation, () => {
                HavokInterop.AddHavokAnimation( File.HkxTempLocation, ImportPath, Index, File.HkxTempLocation );
            } ) );
            File.Command.Add( command );

            UiUtils.OkNotification( "Havok data imported" );
        }
    }
}
