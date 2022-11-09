using VfxEditor.AVFXLib.Binder;

namespace VfxEditor.AvfxFormat.Vfx {
    public class UiBinderDataLinear : UiData {
        public UiBinderDataLinear( AVFXBinderDataLinear data ) {
            Tabs.Add( new UiCurve( data.CarryOverFactor, "Carry Over Factor" ) );
            Tabs.Add( new UiCurve( data.CarryOverFactorRandom, "Carry Over Factor Random" ) );
        }
    }
}