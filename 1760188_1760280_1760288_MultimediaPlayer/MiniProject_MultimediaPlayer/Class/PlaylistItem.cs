using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject_MultimediaPlayer
{
    public class PlaylistItem
    {
        public string fileName { get; set; }
        public string filePath { get; set; }
        public bool isChecked { get; set; }
        public int locationMusic { get; set; }
    }
}
