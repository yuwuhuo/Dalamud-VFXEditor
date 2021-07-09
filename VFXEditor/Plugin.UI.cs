using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using VFXEditor.Structs.Vfx;
using VFXEditor.UI;
using VFXEditor.UI.Graphics;
using VFXEditor.UI.VFX;
using VFXSelect.UI;

namespace VFXEditor {
    public partial class Plugin {
        public bool Visible = false;

        public UIMain CurrentVFXUI;
        public VFXSelectDialog SelectUI;
        public VFXSelectDialog PreviewUI;
        public TexToolsDialog TexToolsUI;
        public PenumbraDialog PenumbraUI;
        public DocDialog DocUI;
        public TextureDialog TextureUI;
        public VFXManipulator VFXManip;

        public BaseVfx SpawnVfx;

        private string IconText;
        private string StatusText;
        private Vector4 StatusColor;

        private string RawInputValue = "";
        private string RawTexInputValue = "";
        public DateTime LastUpdate = DateTime.Now;

        public void InitUI() {
            SelectUI = new VFXSelectDialog(
                Sheets, "File Select [SOURCE]",
                Configuration.RecentSelects,
                showSpawn: true,
                spawnVfxExists: () => SpawnExists(),
                removeSpawnVfx: () => RemoveSpawnVfx(),
                spawnOnGround: ( string path ) => SpawnOnGround( path ),
                spawnOnSelf: ( string path ) => SpawnOnSelf( path ),
                spawnOnTarget: ( string path ) => SpawnOnTarget( path )
            );
            PreviewUI = new VFXSelectDialog(
                Sheets, "File Select [TARGET]",
                Configuration.RecentSelects,
                showSpawn: true,
                spawnVfxExists: () => SpawnExists(),
                removeSpawnVfx: () => RemoveSpawnVfx(),
                spawnOnGround: ( string path ) => SpawnOnGround( path ),
                spawnOnSelf: ( string path ) => SpawnOnSelf( path ),
                spawnOnTarget: ( string path ) => SpawnOnTarget( path )
            );

            SelectUI.OnSelect += SetSourceVFX;
            PreviewUI.OnSelect += SetReplaceVFX;

            TexToolsUI = new TexToolsDialog( this );
            PenumbraUI = new PenumbraDialog( this );
            DocUI = new DocDialog( this );
            TextureUI = new TextureDialog( this );
            VFXManip = new VFXManipulator( this );

#if DEBUG
            Visible = true;
#endif
        }

        public void RefreshVFXUI() {
            CurrentVFXUI = new UIMain( AVFX, this );
        }

        public bool SpawnExists() {
            return SpawnVfx != null;
        }

        public void RemoveSpawnVfx() {
            SpawnVfx?.Remove();
            SpawnVfx = null;
        }

        public void SpawnOnGround( string path ) {
            SpawnVfx = new StaticVfx( this, path, PluginInterface.ClientState.LocalPlayer.Position );
        }

        public void SpawnOnSelf( string path ) {
            SpawnVfx = new ActorVfx( this, PluginInterface.ClientState.LocalPlayer, PluginInterface.ClientState.LocalPlayer, path );
        }

        public void SpawnOnTarget( string path ) {
            var t = PluginInterface.ClientState.Targets.CurrentTarget;
            if( t != null ) {
                SpawnVfx = new ActorVfx( this, t, t, path );
            }
        }

        public void DrawUI() {
            if( Visible ) {
                DrawMainInterface();
            }
            SelectUI.Draw();
            PreviewUI.Draw();
            TexToolsUI.Draw();
            PenumbraUI.Draw();
            DocUI.Draw();
            TextureUI.Draw();
            VFXManip.Draw();
        }

        public void DrawMainInterface() {
            ImGui.SetNextWindowSize( new Vector2( 800, 1000 ), ImGuiCond.FirstUseEver );
            if( !ImGui.Begin( Name, ref Visible, ImGuiWindowFlags.MenuBar ) ) return;

            ImGui.BeginTabBar( "MainInterfaceTabs" );
            DrawMainTab();
            DrawExtract();
            DrawSettings();
            ImGui.EndTabBar();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( CurrentVFXUI == null ) {
                ImGui.Text( @"Select a source VFX file to begin..." );
            }
            else {
                ImGui.PushStyleColor( ImGuiCol.Button, new Vector4( 0.10f, 0.80f, 0.10f, 1.0f ) );
                if( ImGui.Button( "UPDATE" ) ) {
                    if( ( DateTime.Now - LastUpdate ).TotalSeconds > 0.5 ) { // only allow updates every 1/2 second
                        Doc.Save();
                        ResourceLoader.ReRender();
                        LastUpdate = DateTime.Now;
                    }
                }
                ImGui.PopStyleColor();
                // ===== EXPORT ======
                ImGui.SameLine();
                ImGui.PushFont( UiBuilder.IconFont );
                if( ImGui.Button( $"{( char )FontAwesomeIcon.FileDownload}" ) ) {
                    ImGui.OpenPopup( "Export_Popup" );
                }
                ImGui.PopFont();

                if( ImGui.BeginPopup( "Export_Popup" ) ) {
                    if( ImGui.Selectable( ".AVFX" ) ) {
                        var node = AVFX.ToAVFX();
                        SaveDialog( "AVFX File (*.avfx)|*.avfx*|All files (*.*)|*.*", node.ToBytes(), "avfx" );
                    }
                    if( ImGui.Selectable( "TexTools Mod" ) ) {
                        TexToolsUI.Show();
                    }
                    if( ImGui.Selectable( "Penumbra Mod" ) ) {
                        PenumbraUI.Show();
                    }
                    if( ImGui.Selectable( "Export last import (raw)" ) ) {
                        SaveDialog( "TXT files (*.txt)|*.txt|All files (*.*)|*.*", LastImportNode.ExportString( 0 ), "txt" );
                    }
                    ImGui.EndPopup();
                }
                // ======= TEXTURES ==========
                ImGui.SameLine();
                ImGui.PushFont( UiBuilder.IconFont );
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Image}" ) ) {
                    TextureUI.Show();
                }
                ImGui.PopFont();

                ImGui.SameLine();
                ImGui.Text( $"{TexManager.GamePathReplace.Count} Texture(s)" );

                // ======== VERIFY ============
                ImGui.SameLine();
                if( Configuration.Config.VerifyOnLoad ) {
                    ImGui.SameLine();
                    ImGui.PushFont( UiBuilder.IconFont );
                    ImGui.TextColored( StatusColor, IconText );
                    ImGui.PopFont();
                    ImGui.SameLine();
                    ImGui.TextColored( StatusColor, StatusText );
                }

                ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
                CurrentVFXUI.Draw();
            }
            ImGui.End();
        }

        public void SetStatus( bool status ) {
            if( status ) {
                IconText = $"{( char )FontAwesomeIcon.Check}";
                StatusText = "Verified";
                StatusColor = new Vector4( 0.15f, 0.90f, 0.15f, 1.0f );
            }
            else {
                IconText = $"{( char )FontAwesomeIcon.Times}";
                StatusText = "Parsing Issue";
                StatusColor = new Vector4( 0.90f, 0.15f, 0.15f, 1.0f );
            }
        }

        public void DrawMainTab() {
            if( !ImGui.BeginTabItem( "Main##MainInterfaceTabs" ) ) return;

            if(ImGui.BeginMenuBar()) {
                if(ImGui.BeginMenu("File##Menu")) {
                    ImGui.TextDisabled( "Workspace" );
                    ImGui.SameLine();
                    HelpMarker( "A workspace allows you to save multiple vfx replacements at the same time, as well as any imported textures or item renaming (such as particles or emitters)" );

                    if(ImGui.MenuItem("New##Menu")) {
                        // NEW
                    }
                    if( ImGui.MenuItem( "Open##Menu" ) ) {
                        // OPEN
                    }
                    if( ImGui.MenuItem( "Save##Menu" ) ) {
                        // SAVE
                    }
                    if( ImGui.MenuItem( "Save As##Menu" ) ) {
                        // SAVE AS
                    }

                    ImGui.TextDisabled( "Documents" );
                    if( ImGui.MenuItem( "View all Documents##Menu" ) ) {
                        DocUI.Show();
                    }
                    if( ImGui.MenuItem( "View Imported Textures##Menu" ) ) {
                        TextureUI.Show();
                    }

                    ImGui.EndMenu();
                }
                if( ImGui.BeginMenu( "Help##Menu" ) ) {
                    if( ImGui.MenuItem( "Report an Issue##Menu" ) ) {
                        Process.Start( "https://github.com/0ceal0t/Dalamud-VFXEditor/issues" );
                    }
                    if( ImGui.MenuItem( "Baisc Guide##Menu" ) ) {
                        Process.Start( "https://github.com/0ceal0t/Dalamud-VFXEditor/wiki/Basic-Guide" );
                    }
                    if( ImGui.MenuItem( "Github##Menu" ) ) {
                        Process.Start( "https://github.com/0ceal0t/Dalamud-VFXEditor" );
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            ImGui.PushStyleColor( ImGuiCol.ChildBg, new Vector4( 0.18f, 0.18f, 0.22f, 0.4f ) );
            ImGui.SetCursorPos( ImGui.GetCursorPos() - new Vector2( 5, 5 ) );
            ImGui.BeginChild( "Child##MainInterface", new Vector2( ImGui.GetWindowWidth() - 0, 60 ) );
            ImGui.SetCursorPos( ImGui.GetCursorPos() + new Vector2( 5, 5 ) );
            ImGui.PopStyleColor();

            ImGui.Columns( 3, "MainInterfaceFileColumns", false );

            // ======== INPUT TEXT =========
            ImGui.SetColumnWidth( 0, 95 );
            ImGui.Text( "Data Source" );
            ImGui.SameLine(); HelpMarker( "The source of the new VFX. For example, if you wanted to replace the Fire animation with that of Blizzard, Blizzard would be the data source" );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Text( "Preview On" );
            ImGui.SameLine(); HelpMarker( "The VFX which is being replaced. For example, if you wanted to replace the Fire animation with that of Blizzard, Fire would be the preview vfx" );
            ImGui.NextColumn();

            // ======= SEARCH BARS =========
            string sourceString = SourceString;
            string previewString = ReplaceString;
            ImGui.SetColumnWidth( 1, ImGui.GetWindowWidth() - 230 );
            ImGui.PushItemWidth( ImGui.GetColumnWidth() - 100 );

            ImGui.InputText( "##MainInterfaceFiles-Source", ref sourceString, 255, ImGuiInputTextFlags.ReadOnly );

            ImGui.SameLine();
            ImGui.PushFont( UiBuilder.IconFont );
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Search}", new Vector2( 30, 23 ) ) ) {
                SelectUI.Show();
            }
            ImGui.SameLine();
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            ImGui.PushStyleColor( ImGuiCol.Button, new Vector4( 0.80f, 0.10f, 0.10f, 1.0f ) );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Times}##MainInterfaceFiles-SourceRemove", new Vector2( 30, 23 ) ) ) {
                RemoveSourceVFX();
            }
            ImGui.PopStyleColor();
            ImGui.PopFont();


            ImGui.InputText( "##MainInterfaceFiles-Preview", ref previewString, 255, ImGuiInputTextFlags.ReadOnly );

            ImGui.SameLine();
            ImGui.PushFont( UiBuilder.IconFont );
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Search}##MainInterfaceFiles-PreviewSelect", new Vector2( 30, 23 ) ) ) {
                PreviewUI.Show( showLocal: false );
            }
            ImGui.SameLine();
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            ImGui.PushStyleColor( ImGuiCol.Button, new Vector4( 0.80f, 0.10f, 0.10f, 1.0f ) );
            if( ImGui.Button( $"{( char )FontAwesomeIcon.Times}##MainInterfaceFiles-PreviewRemove", new Vector2( 30, 23 ) ) ) {
                RemoveReplaceVFX();
            }
            ImGui.PopStyleColor();
            ImGui.PopFont();

            ImGui.PopItemWidth();

            // ======= TEMPLATES + OVERLAY =========
            ImGui.NextColumn();
            ImGui.SetColumnWidth( 3, 200 );

            if( ImGui.Button( $"Templates", new Vector2( 70, 23 ) ) ) {
                ImGui.OpenPopup( "New_Popup1" );
            }

            if( ImGui.BeginPopup( "New_Popup1" ) ) {
                if( ImGui.Selectable( "Blank" ) ) {
                    OpenTemplate( @"default_vfx.avfx" );
                }
                if( ImGui.Selectable( "Weapon" ) ) {
                    OpenTemplate( @"default_weapon.avfx" );
                }
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 6 );
            ImGui.PushFont( UiBuilder.IconFont );
            if( ImGui.Button( $"{( !Tracker.Enabled ? ( char )FontAwesomeIcon.Eye : ( char )FontAwesomeIcon.EyeSlash )}##MainInterfaceFiles-MarkVfx", new Vector2( 28, 23 ) ) ) {
                Tracker.Enabled = !Tracker.Enabled;
                if( !Tracker.Enabled ) {
                    Tracker.Reset();
                    PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
                }
                else {
                    PluginInterface.UiBuilder.DisableCutsceneUiHide = true;
                }
            }
            ImGui.PopFont();

            ImGui.SameLine(); HelpMarker( @"Use the eye icon to enable or disable the VFX overlay. This will show you the positions of most VFXs in the game world, along with their file paths. Note that you may need to enter and exit your current zone to see all of the VFXs" );

            // =======SPAWN + MANIP =========
            string previewSpawn = Doc.ActiveDoc.Replace.Path;
            bool spawnDisabled = string.IsNullOrEmpty( previewSpawn );
            if( !SpawnExists() ) {
                if( spawnDisabled ) {
                    ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                }
                if( ImGui.Button( "Spawn", new Vector2( 70, 23 ) ) && !spawnDisabled ) {
                    ImGui.OpenPopup( "Spawn_Popup" );
                }
                if( spawnDisabled ) {
                    ImGui.PopStyleVar();
                }
            }
            else {
                if( ImGui.Button( "Remove" ) ) {
                    RemoveSpawnVfx();
                }
            }
            if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) {
                    SpawnOnGround( previewSpawn );
                }
                if( ImGui.Selectable( "On Self" ) ) {
                    SpawnOnSelf( previewSpawn );
                }
                if( ImGui.Selectable( "On Taget" ) ) {
                    SpawnOnTarget( previewSpawn );
                }
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 6 );
            ImGui.PushFont( UiBuilder.IconFont );
            if( !VFXManip.CanBeEnabled ) {
                ImGui.PushStyleVar( ImGuiStyleVar.Alpha, 0.5f );
                ImGui.Button( $"{( char )FontAwesomeIcon.Cube}##MainInterfaceFiles-VfxManip", new Vector2( 28, 23 ) );
                ImGui.PopStyleVar();
            }
            else {
                if( ImGui.Button( $"{( char )FontAwesomeIcon.Cube}##MainInterfaceFiles-VfxManip", new Vector2( 28, 23 ) ) ) {
                    VFXManip.Enabled = !VFXManip.Enabled;
                }
            }
            ImGui.PopFont();

            ImGui.SameLine(); HelpMarker( @"Use the cube icon to enable the manipulator for VFXs spawned on the ground. Note that this feature is very experimental, and will not work for most VFXs" );

            ImGui.Columns( 1 );
            ImGui.Separator();
            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        public void DrawExtract() {
            if( !ImGui.BeginTabItem( "Extract##MainInterfaceTabs" ) ) return;
            // ======= AVFX =========
            ImGui.Text( "Extract a raw .avfx file" );
            ImGui.InputText( "Path##RawExtract", ref RawInputValue, 255 );
            ImGui.SameLine();
            if( ImGui.Button( "Extract##RawExtract" ) ) {
                bool result = PluginInterface.Data.FileExists( RawInputValue );
                if( result ) {
                    try {
                        var file = PluginInterface.Data.GetFile( RawInputValue );
                        SaveDialog( "AVFX File (*.avfx)|*.avfx*|All files (*.*)|*.*", file.Data, "avfx" );
                    }
                    catch( Exception e ) {
                        PluginLog.LogError( "Could not read file" );
                        PluginLog.LogError( e.ToString() );
                    }
                }
            }
            // ===== ATEX ==========
            ImGui.Text( "Extract an .atex file" );
            ImGui.InputText( "Path##RawTexExtract", ref RawTexInputValue, 255 );
            ImGui.SameLine();
            if( ImGui.Button( "Extract##RawTexExtract" ) ) {
                bool result = PluginInterface.Data.FileExists( RawTexInputValue );
                if( result ) {
                    try {
                        var file = PluginInterface.Data.GetFile( RawTexInputValue );
                        SaveDialog( "ATEX File (*.atex)|*.atex*|All files (*.*)|*.*", file.Data, "atex" );
                    }
                    catch( Exception e ) {
                        PluginLog.LogError( "Could not read file" );
                        PluginLog.LogError( e.ToString() );
                    }
                }
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Separator();
            ImGui.EndTabItem();
        }

        public void DrawSettings() {
            if( !ImGui.BeginTabItem( "Settings##MainInterfaceTabs" ) ) return;

            ImGui.Text( "Changes to the temp file location may require a restart to take effect" );
            ImGui.InputText( "Temp file location", ref Configuration.Config.WriteLocation, 255 );
            ImGui.SetNextItemWidth( 200 );
            ImGui.Checkbox( "Verify on load##Settings", ref Configuration.Config.VerifyOnLoad );
            ImGui.SameLine();
            ImGui.Checkbox( "Log all files##Settings", ref Configuration.Config.LogAllFiles );
            ImGui.SameLine();
            ImGui.Checkbox( "Hide with UI##Settings", ref Configuration.Config.HideWithUI );
            ImGui.SameLine();
            ImGui.SetNextItemWidth( 135 );
            if( ImGui.InputInt( "Recent VFX Limit##Settings", ref Configuration.Config.SaveRecentLimit ) ) {
                Configuration.Config.SaveRecentLimit = Math.Max( Configuration.Config.SaveRecentLimit, 0 );
            }
            ImGui.Checkbox( "Live Overlay Limit by Distance##Settings", ref Configuration.Config.OverlayLimit );

            if( ImGui.Button( "Save##Settings" ) ) {
                Configuration.Config.Save();
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Separator();
            ImGui.EndTabItem();
        }

        // ======= HELPERS ============
        public void OpenTemplate( string path ) {
            VFXSelectResult newResult = new VFXSelectResult();
            newResult.DisplayString = "[NEW]";
            newResult.Type = VFXSelectType.Local;
            newResult.Path = Path.Combine( TemplateLocation, "Files", path );
            SetSourceVFX( newResult );
        }

        public void SaveDialog( string filter, string data, string ext ) {
            SaveDialog( filter, Encoding.ASCII.GetBytes( data ), ext );
        }

        public void SaveDialog( string filter, byte[] data, string ext ) {
            SaveFileDialog( filter, "Select a Save Location.", ext,
                ( string path ) => {
                    try {
                        File.WriteAllBytes( path, data );
                    }
                    catch( Exception ex ) {
                        PluginLog.LogError( ex, "Could not save to: " + path );
                    }
                }
            );
        }

        // =========== HELPERS ===========
        public static void HelpMarker(string text) {
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            ImGui.TextDisabled( "(?)" );
            if(ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos( ImGui.GetFontSize() * 35.0f );
                ImGui.TextUnformatted( text );
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        public static void ImportFileDialog(string filter, string title, Action<string> callback) {
            Task.Run( async () => {
                var picker = new OpenFileDialog
                {
                    Filter = filter,
                    CheckFileExists = true,
                    Title = title
                };
                var result = await picker.ShowDialogAsync();
                if(result == DialogResult.OK) {
                    callback( picker.FileName );
                }
            } );
        }

        public static void SaveFileDialog( string filter, string title, string defaultExt, Action<string> callback ) {
            Task.Run( async () => {
                var picker = new SaveFileDialog
                {
                    Filter = filter,
                    DefaultExt = defaultExt,
                    AddExtension = true,
                    Title = title
                };
                var result = await picker.ShowDialogAsync();
                if( result == DialogResult.OK ) {
                    callback( picker.FileName );
                }
            } );
        }

        public static void SaveFolderDialog( string filter, string title, Action<string> callback ) {
            Task.Run( async () => {
                var picker = new SaveFileDialog
                {
                    Filter = filter,
                    Title = title
                };
                var result = await picker.ShowDialogAsync();
                if( result == DialogResult.OK ) {
                    callback( picker.FileName );
                }
            } );
        }
    }
}