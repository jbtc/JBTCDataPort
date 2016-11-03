using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    
public class FilesAndFolders
    {

        /// <summary>
        /// Write content to file.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fname">The fname.</param>
        public static void WritetoFile(string message, string fname)
        {
            File.WriteAllText(fname, message);
        }

        /// <summary>
        /// Get the noOfLines last lines from file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="noOfLines">The no of lines.</param>
        /// <returns></returns>
        public static List<string> getlastLines(string file, int noOfLines)
        {
            List<string> text = File.ReadLines(file).Reverse().Take(noOfLines).ToList();
            return text;
        }

        /// <summary>
        /// Reads the end tokens.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="numberOfTokens">The number of tokens.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="tokenSeparator">The token separator.</param>
        /// <returns></returns>
        public static string ReadEndTokens(string path, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
        {

            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(tokenSeparator);


            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Int64 tokenCount = 0;
                Int64 endPosition = fs.Length / sizeOfChar;

                for (Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);

                    if (encoding.GetString(buffer) == tokenSeparator)
                    {
                        tokenCount++;
                        if (tokenCount == numberOfTokens)
                        {
                            byte[] returnBuffer = new byte[fs.Length - fs.Position];
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            return encoding.GetString(returnBuffer);
                        }
                    }
                }

                // handle case where number of tokens in file is less than numberOfTokens
                fs.Seek(0, SeekOrigin.Begin);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return encoding.GetString(buffer);
            }
        }

        /// <summary>
        /// Gets the subdirectories containing only files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return from subdirectory in Directory.GetDirectories(path, "*", 
                SearchOption.AllDirectories)
                   where Directory.GetDirectories(subdirectory).Length == 0
                   select subdirectory;
        }

        /// <summary>
        /// check for termination request
        /// </summary>
        /// <returns></returns>
        public static bool checkForTermination()
        {
            return System.IO.File.Exists(@".\Stop.txt");
        }
    }
}