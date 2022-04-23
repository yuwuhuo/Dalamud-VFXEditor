using System;
using VFXEditor.AVFX.VFX;
using VFXEditor.FileManager;
using VFXSelect;
using VFXSelect.VFX;

namespace VFXEditor.AVFX {
    public class AVFXManager : FileManager<AVFXDocument, WorkspaceMetaAvfx, AVFXFile> {
        public static VFXSelectDialog SourceSelect { get; private set; }
        public static VFXSelectDialog ReplaceSelect { get; private set; }

        public static void Setup() {
            SourceSelect = new VFXSelectDialog(
                "File Select [SOURCE]",
                Plugin.Configuration.RecentSelects,
                true,
                SetSourceGlobal,
                showSpawn: true,
                spawnVfxExists: Plugin.SpawnExists,
                removeSpawnVfx: Plugin.RemoveSpawnVfx,
                spawnOnGround: Plugin.SpawnOnGround,
                spawnOnSelf: Plugin.SpawnOnSelf,
                spawnOnTarget: Plugin.SpawnOnTarget
            );
            ReplaceSelect = new VFXSelectDialog(
                "File Select [TARGET]",
                Plugin.Configuration.RecentSelects,
                false,
                SetReplaceGlobal,
                showSpawn: true,
                spawnVfxExists: Plugin.SpawnExists,
                removeSpawnVfx: Plugin.RemoveSpawnVfx,
                spawnOnGround: Plugin.SpawnOnGround,
                spawnOnSelf: Plugin.SpawnOnSelf,
                spawnOnTarget: Plugin.SpawnOnTarget
            );
        }

        public static void SetSourceGlobal( SelectResult result ) {
            Plugin.AvfxManager?.SetSource( result );
            Plugin.Configuration.AddRecent( result );
        }

        public static void SetReplaceGlobal( SelectResult result ) {
            Plugin.AvfxManager?.SetReplace( result );
            Plugin.Configuration.AddRecent( result );
        }

        public static readonly string PenumbraPath = "VFX";
        
        // =================

        public AVFXManager() : base( title: "VFXEditor", id: "Vfx", tempFilePrefix: "VfxTemp", extension: "avfx", penumbaPath: PenumbraPath ) { }

        protected override AVFXDocument GetNewDocument() => new( LocalPath );

        protected override AVFXDocument GetImportedDocument( string localPath, WorkspaceMetaAvfx data ) => new( LocalPath, localPath, data );

        public override void Dispose() {
            base.Dispose();
            SourceSelect.Hide();
            ReplaceSelect.Hide();
        }

        public override void DrawBody() {
            SourceSelect.Draw();
            ReplaceSelect.Draw();
            base.DrawBody();
        }
    }
}
