using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using VfxEditor.FileManager;
using VfxEditor.Parsing;
using VfxEditor.Utils;

namespace VfxEditor.ScdFormat {
    public class SoundRandomTracks {
        public List<RandomTrackInfo> Tracks = new();
        public readonly ParsedInt CycleInterval = new( "Cycle Interval" );
        public readonly ParsedShort CycleNumPlayTrack = new( "Cycle Play Track" );
        public readonly ParsedShort CycleRange = new( "Cycle Range" );

        public void Read( BinaryReader reader, SoundType type, byte trackCount ) {
            for( var i = 0; i < trackCount; i++ ) {
                var newTrack = new RandomTrackInfo();
                newTrack.Read( reader );
                Tracks.Add( newTrack );
            }

            if( type == SoundType.Cycle ) {
                CycleInterval.Read( reader );
                CycleNumPlayTrack.Read( reader );
                CycleRange.Read( reader );
            }
        }

        public void Write( BinaryWriter writer, SoundType type ) {
            Tracks.ForEach( x => x.Write( writer ) );

            if( type == SoundType.Cycle ) {
                CycleInterval.Write( writer );
                CycleNumPlayTrack.Write( writer );
                CycleRange.Write( writer );
            }
        }

        public void Draw( SoundType type ) {
            using var _ = ImRaii.PushId( "Tracks" );

            if( type == SoundType.Cycle ) {
                CycleInterval.Draw( CommandManager.Scd );
                CycleNumPlayTrack.Draw( CommandManager.Scd );
                CycleRange.Draw( CommandManager.Scd );
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 3 );

            for( var idx = 0; idx < Tracks.Count; idx++ ) {
                if( ImGui.CollapsingHeader( $"Track #{idx}", ImGuiTreeNodeFlags.DefaultOpen ) ) {
                    using var __ = ImRaii.PushId( idx );
                    using var indent = ImRaii.PushIndent();

                    if( UiUtils.RemoveButton( "Delete", true ) ) { // REMOVE
                        CommandManager.Scd.Add( new GenericRemoveCommand<RandomTrackInfo>( Tracks, Tracks[idx] ) );
                        break;
                    }

                    Tracks[idx].Draw();
                    ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 3 );
                }
            }

            if( ImGui.Button( "+ New" ) ) { // NEW
                CommandManager.Scd.Add( new GenericAddCommand<RandomTrackInfo>( Tracks, new RandomTrackInfo() ) );
            }
        }
    }

    public class RandomTrackInfo {
        public readonly SoundTrackInfo Track = new();
        public readonly ParsedInt UpperLimit = new( "Upper Limit" );

        public void Read( BinaryReader reader ) {
            Track.Read( reader );
            UpperLimit.Read( reader );
        }

        public void Write( BinaryWriter writer ) {
            Track.Write( writer );
            UpperLimit.Write( writer );
        }

        public void Draw() {
            Track.Draw();
            UpperLimit.Draw( CommandManager.Scd );
        }
    }
}
