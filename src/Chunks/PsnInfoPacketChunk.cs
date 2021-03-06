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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	/// <summary>
	///     Root chunk of a PosiStageNet info packet
	/// </summary>
	[PublicAPI]
	public sealed class PsnInfoPacketChunk : PsnPacketChunk
	{
		public PsnInfoPacketChunk([NotNull] IEnumerable<PsnInfoPacketSubChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		public PsnInfoPacketChunk(params PsnInfoPacketSubChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		private PsnInfoPacketChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public override int DataLength => 0;

		public override PsnPacketChunkId ChunkId => PsnPacketChunkId.PsnInfoPacket;

		public IEnumerable<PsnInfoPacketSubChunk> SubChunks => RawSubChunks.OfType<PsnInfoPacketSubChunk>();

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoPacketChunk),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnInfoPacketChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnInfoPacketChunkId)pair.Item1.ChunkId)
				{
					case PsnInfoPacketChunkId.PsnInfoHeader:
						subChunks.Add(PsnInfoHeaderChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnInfoPacketChunkId.PsnInfoSystemName:
						subChunks.Add(PsnInfoSystemNameChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnInfoPacketChunkId.PsnInfoTrackerList:
						subChunks.Add(PsnInfoTrackerListChunk.Deserialize(pair.Item1, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(pair.Item1, reader));
						break;
				}
			}

			return new PsnInfoPacketChunk(subChunks);
		}
	}



	[PublicAPI]
	public abstract class PsnInfoPacketSubChunk : PsnChunk
	{
		protected PsnInfoPacketSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public abstract PsnInfoPacketChunkId ChunkId { get; }
		public override ushort RawChunkId => (ushort)ChunkId;
	}



	[PublicAPI]
	public sealed class PsnInfoHeaderChunk : PsnInfoPacketSubChunk, IEquatable<PsnInfoHeaderChunk>
	{
		public const int StaticChunkAndHeaderLength = ChunkHeaderLength + StaticDataLength;
		public const int StaticDataLength = 12;

		public PsnInfoHeaderChunk(ulong timestamp, int versionHigh, int versionLow, int frameId, int framePacketCount)
			: base(null)
		{
			TimeStamp = timestamp;

			if (versionHigh < 0 || versionHigh > 255)
				throw new ArgumentOutOfRangeException(nameof(versionHigh), "versionHigh must be between 0 and 255");

			VersionHigh = versionHigh;

			if (versionLow < 0 || versionLow > 255)
				throw new ArgumentOutOfRangeException(nameof(versionLow), "versionLow must be between 0 and 255");

			VersionLow = versionLow;

			if (frameId < 0 || frameId > 255)
				throw new ArgumentOutOfRangeException(nameof(frameId), "frameId must be between 0 and 255");

			FrameId = frameId;

			if (framePacketCount < 0 || framePacketCount > 255)
				throw new ArgumentOutOfRangeException(nameof(framePacketCount), "framePacketCount must be between 0 and 255");

			FramePacketCount = framePacketCount;
		}

		public ulong TimeStamp { get; }

		public int VersionHigh { get; }
		public int VersionLow { get; }
		public int FrameId { get; }
		public int FramePacketCount { get; }

		public override int DataLength => StaticDataLength;

		public override PsnInfoPacketChunkId ChunkId => PsnInfoPacketChunkId.PsnInfoHeader;

		public bool Equals([CanBeNull] PsnInfoHeaderChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && TimeStamp == other.TimeStamp && VersionHigh == other.VersionHigh
			       && VersionLow == other.VersionLow && FrameId == other.FrameId && FramePacketCount == other.FramePacketCount;
		}

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoHeaderChunk),
				new XAttribute(nameof(TimeStamp), TimeStamp),
				new XAttribute(nameof(VersionHigh), VersionHigh),
				new XAttribute(nameof(FrameId), FrameId),
				new XAttribute(nameof(FramePacketCount), FramePacketCount));
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnInfoHeaderChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ TimeStamp.GetHashCode();
				hashCode = (hashCode * 397) ^ VersionHigh;
				hashCode = (hashCode * 397) ^ VersionLow;
				hashCode = (hashCode * 397) ^ FrameId;
				hashCode = (hashCode * 397) ^ FramePacketCount;
				return hashCode;
			}
		}

		internal static PsnInfoHeaderChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			ulong timeStamp = reader.ReadUInt64();
			int versionHigh = reader.ReadByte();
			int versionLow = reader.ReadByte();
			int frameId = reader.ReadByte();
			int framePacketCount = reader.ReadByte();

			return new PsnInfoHeaderChunk(timeStamp, versionHigh, versionLow, frameId, framePacketCount);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(TimeStamp);
			writer.Write((byte)VersionHigh);
			writer.Write((byte)VersionLow);
			writer.Write((byte)FrameId);
			writer.Write((byte)FramePacketCount);
		}
	}
}