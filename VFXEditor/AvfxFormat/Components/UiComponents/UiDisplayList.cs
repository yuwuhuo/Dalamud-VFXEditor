using OtterGui.Raii;
using System;
using System.Collections.Generic;
using System.IO;
using VfxEditor.Ui.Interfaces;

namespace VfxEditor.AvfxFormat {
    public class UiDisplayList : AvfxItem {
        public readonly string Name;
        private readonly List<IUiItem> Display;

        public UiDisplayList( string name ) : this( name, new List<IUiItem>() ) { }


        public UiDisplayList( string name, List<IUiItem> list ) : base( "" ) {
            Name = name;
            Display = list;
            SetAssigned( true );
        }

        public void Add( IUiItem item ) => Display.Add( item );

        public void AddRange( List<IUiItem> items ) => Display.AddRange( items );

        public void Remove( IUiItem item ) => Display.Remove( item );

        public void Prepend( IUiItem item ) => Display.Insert( 0, item );

        public override void Draw() {
            using var _ = ImRaii.PushId( Name );
            using var child = ImRaii.Child( "Child" );
            DrawItems( Display );
        }

        public override string GetDefaultText() => Name;

        protected override void RecurseChildrenAssigned( bool assigned ) {
            throw new NotImplementedException();
        }

        public override void ReadContents( BinaryReader reader, int size ) {
            throw new NotImplementedException();
        }

        protected override void WriteContents( BinaryWriter writer ) {
            throw new NotImplementedException();
        }
    }
}
