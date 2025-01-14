using System;
using System.Collections.Generic;
using VfxEditor.FileManager;
using VfxEditor.Ui.Interfaces;

namespace VfxEditor.Ui.Components {
    public class SimpleDropdown<T> : Dropdown<T> where T : class, IUiItem {
        private readonly Func<T, int, string> GetTextAction;
        private readonly Func<T> NewAction;
        private readonly Func<CommandManager> CommandAction;
        private readonly Action<T> OnChangeAction;

        public SimpleDropdown( string id, List<T> items, Func<T, int, string> getTextAction, Func<T> newAction, Func<CommandManager> commandAction, Action<T> onChangeAction = null ) :
            base( id, items, true, true ) {

            GetTextAction = getTextAction;
            NewAction = newAction;
            CommandAction = commandAction;
            OnChangeAction = onChangeAction;
        }

        protected override void DrawSelected() => Selected.Draw();

        protected override string GetText( T item, int idx ) => GetTextAction == null ? $"{Id} {idx}" : GetTextAction.Invoke( item, idx );

        protected override void OnNew() {
            CommandAction.Invoke().Add( new GenericAddCommand<T>( Items, NewAction.Invoke(), OnChangeAction ) );
        }

        protected override void OnDelete( T item ) {
            CommandAction.Invoke().Add( new GenericRemoveCommand<T>( Items, item, OnChangeAction ) );
        }
    }
}
