#if !DOTNET40
#define Goletas // or C5
#endif
using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;
#if !DOTNET40
#if Goletas
using Goletas.Collections;
#else
using C5;
#endif
#else
using System.Collections.Generic;
#endif

namespace NetTopologySuite.Operation.Union
{
    public class PointGeometryUnion<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IGeometryFactory<TCoordinate> _geomFact;
        private readonly IGeometry<TCoordinate> _otherGeom;
        private readonly IGeometry<TCoordinate> _pointGeom;

        public PointGeometryUnion(IPuntal<TCoordinate> pointGeom, IGeometry<TCoordinate> otherGeom)
        {
            _pointGeom = pointGeom;
            _otherGeom = otherGeom;
            _geomFact = otherGeom.Factory;
        }

        ///<summary>
        /// Computes the union of a <see cref="IPoint{TCoordinate}"/> geometry with 
        /// another arbitrary <see cref="IGeometry{TCoordinate}"/>.
        /// Does not copy any component geometries.
        ///</summary>
        ///<param name="pointGeom"></param>
        ///<param name="otherGeom"></param>
        ///<returns></returns>
        public static IGeometry<TCoordinate> Union(IPuntal<TCoordinate> pointGeom, IGeometry<TCoordinate> otherGeom)
        {
            PointGeometryUnion<TCoordinate> unioner = new PointGeometryUnion<TCoordinate>(pointGeom, otherGeom);
            return unioner.Union();
        }

        public IGeometry<TCoordinate> Union()
        {
            PointLocator<TCoordinate> locater = new PointLocator<TCoordinate>();
            // use a set to eliminate duplicates, as required for union
#if DOTNET40
            ISet<TCoordinate> exteriorCoords = new SortedSet<TCoordinate>();
#else
#if Goletas
            SortedSet<TCoordinate> exteriorCoords = new SortedSet<TCoordinate>();
#else
            TreeSet<TCoordinate> exteriorCoords = new TreeSet<TCoordinate>();
#endif
#endif

            foreach (IPoint<TCoordinate> point in GeometryFilter.Filter<IPoint<TCoordinate>, TCoordinate>(_pointGeom))
            {
                TCoordinate coord = point.Coordinate;
                Locations loc = locater.Locate(coord, _otherGeom);
                if (loc == Locations.Exterior)
                    exteriorCoords.Add(coord);
            }

            // if no points are in exterior, return the other geom
            if (exteriorCoords.Count == 0)
                return _otherGeom;

            // make a puntal geometry of appropriate size
            IGeometry<TCoordinate> ptComp = null;
            ICoordinateSequence<TCoordinate> coords = _geomFact.CoordinateSequenceFactory.Create(exteriorCoords);
            if (coords.Count == 1)
                ptComp = _geomFact.CreatePoint(coords[0]);
            else
                ptComp = _geomFact.CreateMultiPoint(coords);

            // add point component to the other geometry
            return GeometryCombiner<TCoordinate>.Combine(ptComp, _otherGeom);
        }
    }
}