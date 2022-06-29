using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ikkuna
{
	public class Hotkey
	{
		[DllImport("User32")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

		/// <summary>
		/// All allowed Windows modifier keys for global hotkeys.
		/// </summary>
		private static Dictionary<string, int> AllowedModifiers { get; }
			= new Dictionary<string, int>
		{
			{"alt", 0x0001},
			{"ctrl", 0x0002},
			{"control", 0x0002},
			{"shift", 0x0004},
			{"win", 0x0008}
		};

		private static int RegisterIdCounter { get; set; }
			= 1;


		public string Key { get; set;  }
		public int RegisterId { get; set;  }

		// Actions:

		public string MoveToSlot { get; set; }
        public bool ResizeToFillSlot { get; set; }
        public bool MoveLeft { get; set; }
		public bool MoveRight { get; set; }
        public double Resize { get; set; }



        /// <summary>
        /// Creates and registers a new hotkey by a string. For example: "ctrl+alt+e"
        /// </summary>

        public Hotkey()
		{

		}

		public void Register(IntPtr handle)
        {
			RegisterId = RegisterIdCounter;
			var keys = Key.Split('+');
			var modifiers = 0;
			var finalKey = 0;

			var keyIsModifier = false;
			foreach (var key in keys)
			{
				foreach (var modifier in AllowedModifiers)
				{
					if (key.Trim().ToLower() == modifier.Key)
					{
						modifiers += modifier.Value;
						keyIsModifier = true;
						break;
					}
					else
					{
						keyIsModifier = false;
					}
				}
				if (!keyIsModifier) //final key
				{
					Keys keysValue;
					//ignore case with this
					if (Enum.TryParse(key, true, out keysValue))
					{
						finalKey = (int)keysValue;
					}
					else
					{
						throw new Exception("Can't define final key: " + key);
					}
				}
			}

			if (!RegisterHotKey(handle, RegisterId, modifiers, finalKey))
			{
				throw new Exception("Can't register hotkey: " + Key);
			}

			RegisterIdCounter++;
		}

		public bool IsHit(Int32 id)
		{
			return (id == RegisterId);
		}
	}
}
