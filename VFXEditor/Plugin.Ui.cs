using ImGuiNET;
using OtterGui.Raii;
using System;
using System.Collections.Generic;
using VfxEditor.Data;
using VfxEditor.FileManager;
using VfxEditor.TextureFormat;
using VfxEditor.Ui.Components;
using VfxEditor.Ui.Export;
using VfxEditor.Utils;

namespace VfxEditor {
    public partial class Plugin {
        public static readonly Dictionary<string, Modal> Modals = new();

        public static void Draw() {
            if( Loading ) return;

            CopyManager.ResetAll();
            CheckWorkspaceKeybinds();

            TexToolsDialog.Draw();
            PenumbraDialog.Draw();
            ToolsDialog.Draw();
            Tracker.Draw();
            Configuration.Draw();
            LibraryManager.Draw();

            Managers.ForEach( x => x?.Draw() );

            CopyManager.FinalizeAll();

            if( Configuration.AutosaveEnabled &&
                Configuration.AutosaveSeconds > 10 &&
                !string.IsNullOrEmpty( CurrentWorkspaceLocation ) &&
                ( DateTime.Now - LastAutoSave ).TotalSeconds > Configuration.AutosaveSeconds
            ) {
                LastAutoSave = DateTime.Now;
                SaveWorkspace();
            }

            foreach( var modal in Modals ) modal.Value.Draw();
        }

        private static void CheckWorkspaceKeybinds() {
            if( Configuration.OpenKeybind.KeyPressed() ) OpenWorkspace();
            if( Configuration.SaveKeybind.KeyPressed() ) SaveWorkspace();
            if( Configuration.SaveAsKeybind.KeyPressed() ) SaveAsWorkspace();
        }

        public static void DrawFileMenu() {
            using var _ = ImRaii.PushId( "菜单" );

            if( ImGui.BeginMenu( "文件" ) ) {
                ImGui.TextDisabled( "工作区" );
                ImGui.SameLine();
                UiUtils.HelpMarker( "一个工作区允许你同时保存多个VFX替换，导入的贴图或物品的重命名（例如粒子或发射器）。工作区是一个类似于项目或文件夹的容器，可以用来管理VFX替换和其他相关资源。" );

                if( ImGui.MenuItem( "新创建" ) ) NewWorkspace();
                if( ImGui.MenuItem( "打开" ) ) OpenWorkspace();
                if( ImGui.MenuItem( "保存" ) ) SaveWorkspace();
                if( ImGui.MenuItem( "另存为" ) ) SaveAsWorkspace();

                ImGui.Separator();
                if( ImGui.MenuItem( "设置" ) ) Configuration.Show();
                if( ImGui.MenuItem( "工具" ) ) ToolsDialog.Show();
                if( ImGui.BeginMenu( "帮助" ) ) {
                    if( ImGui.MenuItem( "Github" ) ) UiUtils.OpenUrl( "https://github.com/yuwuhuo/Dalamud-VFXEditor" );
                    if( ImGui.MenuItem( "报告问题" ) ) UiUtils.OpenUrl( "https://github.com/0ceal0t/Dalamud-VFXEditor/issues" );
                    if( ImGui.MenuItem( "指南" ) ) UiUtils.OpenUrl( "https://github.com/0ceal0t/Dalamud-VFXEditor/wiki" );
                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if( ImGui.BeginMenu( "导出" ) ) {
                if( ImGui.MenuItem( "Penumbra" ) ) PenumbraDialog.Show();
                if( ImGui.MenuItem( "TexTools" ) ) TexToolsDialog.Show();
                ImGui.EndMenu();
            }
        }

        public static void DrawManagersMenu( IFileManager manager ) {
            using var _ = ImRaii.PushId( "菜单" );

            if( ImGui.MenuItem( "纹理" ) ) TextureManager.Show();
            ImGui.Separator();
            DrawManagerMenu( manager, "Vfx", AvfxManager );
            DrawManagerMenu( manager, "Tmb", TmbManager );
            DrawManagerMenu( manager, "Pap", PapManager );
            DrawManagerMenu( manager, "Scd", ScdManager );
            DrawManagerMenu( manager, "Eid", EidManager );
            DrawManagerMenu( manager, "Uld", UldManager );
            DrawManagerMenu( manager, "Phyb", PhybManager );
        }

        private static void DrawManagerMenu( IFileManager manager, string text, IFileManager menuManager ) {
            using var disabled = ImRaii.Disabled( manager == menuManager );
            if( ImGui.MenuItem( text ) ) menuManager.Show();
        }

        public static void AddModal( Modal modal ) {
            Modals[modal.Title] = modal;
            ImGui.OpenPopup( modal.Title );
        }
    }
}