﻿// Copyright 2008 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NetTopologySuite.IO.SqlServer2008
// The original source is part of of NHibernate.Spatial.
// NetTopologySuite.IO.SqlServer2008 is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// NetTopologySuite.IO.SqlServer2008 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NetTopologySuite.IO.SqlServer2008 if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using GeoAPI.Geometries;
using GeoAPI.IO;
using Microsoft.SqlServer.Types;
using System.IO;

namespace NetTopologySuite.IO
{
    public class MsSql2008GeometryReader : IBinaryGeometryReader, IGeometryReader<SqlGeometry, Stream>
    {
        public IGeometryFactory Factory { get; set; }

        public IGeometry Read(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Read(stream);
            }
        }

        public IGeometry Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var sqlGeometry = new SqlGeometry();
                sqlGeometry.Read(reader);
                return Read(sqlGeometry);
            }
        }

        public IGeometry Read(SqlGeometry geometry)
        {
            var builder = new NtsGeometrySink(Factory);
            geometry.Populate(builder);
            return builder.ConstructedGeometry;
        }


    }
}