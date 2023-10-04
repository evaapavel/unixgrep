using System.Text;

namespace UnixGrepCon
{
    /// <summary>
    /// Helper methods for the file system and files in general.
    /// </summary>
    public static class FileHelper
    {

        /// <summary>The first n bytes to guess whether a file is text or binary.</summary>
        private const int FirstNBytesToGuessWhetherFileIsTextOrBinary = 1000;

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="fileName">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GuessEncoding(string fileName)
        {
            // Read the BOM.
            byte[] bom = new byte[4];
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM.
            //if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            //return Encoding.ASCII;
            return Encoding.UTF8;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="fileName">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static bool GuessIsTextFile(string fileName)
        {
            // Return value.
            // Assume success (meaning the file IS a text file).
            bool result = true;

            // Read first n bytes of the file to make a qualified estimation on whether the file is a text file, or not.
            byte[]? startPartOfFile = null;
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                int numberOfBytesToRead = FirstNBytesToGuessWhetherFileIsTextOrBinary;
                if (file.Length < ((long) int.MaxValue))
                {
                    if (((int) file.Length) < FirstNBytesToGuessWhetherFileIsTextOrBinary)
                    {
                        numberOfBytesToRead = (int) file.Length;
                    }
                }
                startPartOfFile = new byte[numberOfBytesToRead];
                file.Read(startPartOfFile, 0, numberOfBytesToRead);
            }

            // Make frequency statistics for the occurrences of particular bytes in the file.
            Dictionary<byte, int> stats = new Dictionary<byte, int>();
            foreach (byte b in startPartOfFile)
            {
                if (stats.ContainsKey(b))
                {
                    int count = stats[b];
                    count++;
                    stats[b] = count;
                }
                else
                {
                    stats.Add(b, 1);
                }
            }

            // We allow typical control chars such as:
            // 0x00 (null, probably frequent when Unicode with ASCII - eg '0x41' (capital A) would be '0x0041'),
            // 0x09 (horizontal TAB)
            // 0x0A (LF)
            // 0x0D (CR)
            // 0x1B (ESC)
            byte[] allowedBytes = new byte[] { 0x00, 0x09, 0x0A, 0x0D, 0x1B };
            foreach (byte b in stats.Keys)
            {
                // Check for invisible characters only (values between 0 and 31 inclusive).
                if ( (0 <= b) && (b <= 31) )
                {
                    if (!(allowedBytes.Contains(b)))
                    {
                        result = false;
                        break;
                    }
                }
            }

            // Return the result.
            return result;

        }

    }
}
