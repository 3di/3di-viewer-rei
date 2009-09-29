using System;
using System.Collections.Generic;
using System.Text;
using IrrlichtNETCP;
using log4net;

namespace OpenViewer
{
    public interface IRefController
    {
        /// <summary>
        /// OpenViewer 
        /// </summary>
        //IViewer Viewer { get; }

        /// <summary>
        /// A Handle to the Irrlicht Gui manager property.
        /// </summary>
        IrrlichtDevice Device {get;}

        /// <summary>
        /// Irrlicht Video Driver.  A handle to the video driver being used property.
        /// </summary>
        VideoDriver VideoDriver {get;}

        /// <summary>
        /// A handle to the Irrlicht ISceneManager  property.
        /// </summary>
        SceneManager SceneManager {get;}

        /// <summary>
        /// A Handle to the Irrlicht Gui manager property.
        /// </summary>
        GUIEnvironment GUIEnvironment {get;}

        ILog Log {get;}
    }
}