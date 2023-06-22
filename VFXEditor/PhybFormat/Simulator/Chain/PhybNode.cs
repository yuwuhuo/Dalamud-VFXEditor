using System.Collections.Generic;
using System.IO;
using VfxEditor.Parsing;
using VfxEditor.Parsing.String;

namespace VfxEditor.PhybFormat.Simulator.Chain {
    public class PhybNode : PhybData {
        public readonly ParsedPaddedString BoneName = new( "Bone Name", 32, 0xFE );
        public readonly ParsedFloat Radius = new( "Collision Radius" );
        public readonly ParsedFloat AttractByAnimation = new( "Attract by Animation" );
        public readonly ParsedFloat WindScale = new( "Wind Scale" );
        public readonly ParsedFloat GravityScale = new( "Gravity Scale" );
        public readonly ParsedFloat ConeMaxAngle = new( "Cone Max Angle" );
        public readonly ParsedFloat3 ConeAxisOffset = new( "Cone Axis Offset" );
        public readonly ParsedFloat3 ConstraintPlaneNormal = new( "Constraint Plane Normal" );
        public readonly ParsedUInt CollisionFlag = new( "Collision Flags" );
        public readonly ParsedUInt ContinuousCollisionFlag = new( "Continuous Collision Flags" );

        public PhybNode( PhybFile file ) : base( file ) { }

        public PhybNode( PhybFile file, BinaryReader reader ) : base( file, reader ) { }

        protected override List<ParsedBase> GetParsed() => new() {
            BoneName,
            Radius,
            AttractByAnimation,
            WindScale,
            GravityScale,
            ConeMaxAngle,
            ConeAxisOffset,
            ConstraintPlaneNormal,
            CollisionFlag,
            ContinuousCollisionFlag,
        };
    }
}
