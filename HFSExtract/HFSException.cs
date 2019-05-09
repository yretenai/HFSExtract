using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HFSExtract
{
    public class HFSException : Exception
    {
        public HFSError ErrorType { get; }

        public HFSException(HFSError error) : base(ErrorToText(error))
        {
            ErrorType = error;
        }

        private static string ErrorToText(HFSError error)
        {
            switch (error)
            {
                case HFSError.HFSIndexMismatch:
                    return "Can't parse HFS Index";
                case HFSError.HFSFileMismatch:
                    return "Can't parse HFS File";
                case HFSError.HFSDirectoryMismatch:
                    return "Can't parse HFS Directory";
                default:
                    throw new ArgumentOutOfRangeException(nameof(error));
            }
        }
    }
}
