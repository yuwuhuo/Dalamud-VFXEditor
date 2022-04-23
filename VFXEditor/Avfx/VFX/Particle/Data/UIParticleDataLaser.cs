using VFXEditor.AVFXLib.Particle;

namespace VFXEditor.AVFX.VFX {
    public class UIParticleDataLaser : UIData {
        public UIParticleDataLaser( AVFXParticleDataLaser data ) {
            Tabs.Add( new UICurve( data.Width, "Width" ) );
            Tabs.Add( new UICurve( data.Length, "Length" ) );
        }
    }
}
