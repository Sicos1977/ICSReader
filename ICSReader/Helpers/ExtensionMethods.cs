//
// ExtensionMethods.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2021 Magic-Sessions. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

namespace ICSReader.Helpers
{
    public static class ExtensionMethods
    {
        #region StartsWith
        /// <summary>
        /// Returns a value indicating whether a specified byte array starts with the given <param name="value"></param>
        /// </summary>
        /// <param name="source">The source byte array</param>
        /// <param name="offset">The offset</param>
        /// <param name="value">The byte array to seek</param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] source, int offset, byte[] value)
        {
            if (source.Length - offset < value.Length)
                return false;

            for(var i = 0 ; i < value.Length; i ++)
            {
                if (source[i + offset] != value[i])
                    return false;
            }

            return true;
        }
        #endregion
    }
}
