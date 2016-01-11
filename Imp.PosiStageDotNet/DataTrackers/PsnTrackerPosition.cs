﻿// This file is part of PosiStageDotNet.
// 
// PosiStageDotNet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PosiStageDotNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with PosiStageDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet.DataTrackers
{
	public class PsnTrackerPosition : PsnTrackerElement, IEquatable<PsnTrackerPosition>
	{
		public PsnTrackerPosition(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override PsnDataTrackerChunkId Id => PsnDataTrackerChunkId.PsnDataTrackerPos;
		public override int ByteLength => 12;

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public bool Equals(PsnTrackerPosition other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is PsnTrackerPosition && Equals((PsnTrackerPosition)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(PsnTrackerPosition left, PsnTrackerPosition right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PsnTrackerPosition left, PsnTrackerPosition right)
		{
			return !left.Equals(right);
		}

		internal override void Serialize(PsnBinaryWriter writer)
		{
			writer.WriteChunkHeader((ushort)Id, ByteLength, false);
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}
}