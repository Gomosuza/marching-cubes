using System;

namespace MarchingCubes
{
	/// <summary>
	/// Point3 is equivalent to Vector3 but with int values.
	/// </summary>
	public struct Point3 : IEquatable<Point3>
	{
		/// <summary>
		/// The x component.
		/// </summary>
		public int X;

		/// <summary>
		/// The y component.
		/// </summary>
		public int Y;

		/// <summary>
		/// The z component.
		/// </summary>
		public int Z;

		/// <summary>
		/// Creates a new point from the value.
		/// </summary>
		/// <param name="v"></param>
		public Point3(int v)
		{
			X = v;
			Y = v;
			Z = v;
		}

		/// <summary>
		/// Creates a new point from the values.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public Point3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Point3 other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			return $"{X}, {Y}, {Z}";
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
				return false;
			if (!(obj is Point3))
				return false;
			return Equals((Point3)obj);
		}

		/// <summary>
		/// Serves as the default hash function. 
		/// </summary>
		/// <returns>
		/// A hash code for the current object.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = X;
				hashCode = (hashCode * 397) ^ Y;
				hashCode = (hashCode * 397) ^ Z;
				return hashCode;
			}
		}
	}
}