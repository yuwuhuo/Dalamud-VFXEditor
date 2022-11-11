using System.Collections.Generic;
using VfxEditor.AvfxFormat.Views;

namespace VfxEditor.AvfxFormat.Vfx {
    public class UiTimelineItemSequencer : ImGuiSequencer<UiTimelineItem> {
        public UiTimeline Timeline;

        public UiTimelineItemSequencer( List<UiTimelineItem> items, UiTimeline timeline ) : base( items ) {
            Timeline = timeline;
        }

        public override int GetEnd( UiTimelineItem item ) => item.EndTime.Literal.GetValue();

        public override int GetStart( UiTimelineItem item ) => item.StartTime.Literal.GetValue();

        public override UiTimelineItem OnNew() {
            /*var newItem = Timeline.Timeline.AddItem();
            newItem.BinderIdx.SetValue( -1 );
            newItem.EffectorIdx.SetValue( -1 );
            newItem.EmitterIdx.SetValue( -1 );
            newItem.EndTime.SetValue( 1 );
            newItem.Platform.SetValue( 0 );

            return new UiTimelineItem( newItem, Timeline );*/

            return null;
        }

        public override void OnDelete( UiTimelineItem item ) {
            /*item.BinderSelect.Disable();
            item.EmitterSelect.Disable();
            item.EffectorSelect.Disable();
            Timeline.Timeline.RemoveItem( item.Item );*/
        }

        public override void SetEnd( UiTimelineItem item, int end ) => CommandManager.Avfx.Add( new UiIntCommand( item.EndTime.Literal, end ) );

        public override void SetStart( UiTimelineItem item, int start ) => CommandManager.Avfx.Add( new UiIntCommand( item.StartTime.Literal, start ) );

        public override bool IsEnabled( UiTimelineItem item ) => item.Enabled.Literal.GetValue() == true;

        public override void Toggle( UiTimelineItem item ) => CommandManager.Avfx.Add( new UiCheckboxCommand( item.Enabled.Literal, !IsEnabled( item ) ) );
    }
}
