using VfxEditor.Parsing;

namespace VfxEditor.UldFormat.Component.Data {
    public class PreviewComponentData : UldComponentData {
        public PreviewComponentData() {
            Parsed.AddRange( new ParsedBase[] {
                new ParsedUInt( "Unknown 1" ),
                new ParsedUInt( "Unknown 2" ),
            } );
        }
    }
}