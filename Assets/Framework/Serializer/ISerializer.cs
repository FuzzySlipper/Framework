using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
	public interface ISerializer {
		/// <summary>
		/// Serialize object.
		/// </summary>
		/// <param name="obj">The Object to serialize.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>Returns serialized string.</returns>
		string Serialize ( object obj );

		/// <summary>
		/// Deserialize object.
		/// </summary>
		/// <param name="str">The string to deserialize.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>Returns deserialized object.</returns>
		T Deserialize<T> ( string str );
	}
}