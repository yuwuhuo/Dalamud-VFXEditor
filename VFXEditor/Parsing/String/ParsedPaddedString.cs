using System.IO;

namespace VfxEditor.Parsing.String {
    public class ParsedPaddedString : ParsedString {
        private readonly int Length;
        private readonly byte Padding;

        public ParsedPaddedString( string name, string defaultValue, int length, byte padding ) : base( name, defaultValue ) {
            Padding = padding;
            Length = length;
        }

        public ParsedPaddedString( string name, int length, byte padding ) : base( name ) {
            Padding = padding;
            Length = length;
        }

        public override void Read( BinaryReader reader ) => Read( reader, 0 );

        public override void Read( BinaryReader reader, int size ) {
            base.Read( reader, size );
            reader.ReadBytes( Length - Value.Length - 1 );
        }

        public override void Write( BinaryWriter writer ) {
            base.Write( writer );
            for( var i = 0; i < ( Length - Value.Length - 1 ); i++ ) writer.Write( Value.Length == 0 ? ( byte )0 : Padding );
        }

        public override void Draw( CommandManager manager ) => Draw( manager, ( uint )( Length - 1 ) );
    }
}
