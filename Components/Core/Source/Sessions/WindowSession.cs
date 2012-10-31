using System;
using System.Windows;
using System.Windows.Automation;
using Bricks.Core;
using White.Core.Configuration;
using White.Core.Factory;
using White.Core.Logging;
using White.Core.ScreenMap;
using White.Core.UIA;
using White.Core.UIItems;
using White.Core.UIItems.Actions;
using White.Core.UIItems.Container;
using White.Core.UIItems.Finders;
using Window=White.Core.UIItems.WindowItems.Window;

namespace White.Core.Sessions
{
    public class WindowSession : IDisposable
    {
        private readonly ApplicationSession applicationSession;
        private readonly WindowItemsMap windowItemsMap;
        private readonly InitializeOption initializeOption;

        public WindowSession(ApplicationSession applicationSession, InitializeOption initializeOption)
        {
            this.applicationSession = applicationSession;
            windowItemsMap = WindowItemsMap.Create(initializeOption, RectX.UnlikelyWindowPosition);
            if (windowItemsMap.LoadedFromFile) initializeOption.NonCached();
            this.initializeOption = initializeOption;
        }

        protected WindowSession() {}

        public virtual WindowSession ModalWindowSession(InitializeOption modalInitializeOption)
        {
            return applicationSession.WindowSession(modalInitializeOption);
        }

        public virtual IUIItem Get(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener)
        {

            WhiteLogger.Instance.DebugFormat("Finding item based on criteria: ({0}) on ({1})", searchCriteria, initializeOption.Identifier);
            Point location = windowItemsMap.GetItemLocation(searchCriteria);
            if (location.Equals(RectX.UnlikelyWindowPosition))
            {
                WhiteLogger.Instance.Debug("[PositionBasedSearch] Could not find based on position, finding using search.");
                return Create(containerItemFactory, searchCriteria, actionListener, null);
            }

            AutomationElement automationElement = AutomationElementX.GetAutomationElementFromPoint(location);
            if (automationElement != null && searchCriteria.AppliesTo(automationElement))
            {
                IUIItem item = new DictionaryMappedItemFactory().Create(automationElement, actionListener, searchCriteria.CustomItemType);
                return UIItemProxyFactory.Create(item, actionListener);
            }

            WhiteLogger.Instance.DebugFormat("[PositionBasedSearch] UIItem {0} changed its position, finding using search.", searchCriteria);
            return Create(containerItemFactory, searchCriteria, actionListener, null);    

        }

        public virtual IUIItem Get(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener, TimeSpan timeout)
        {

            WhiteLogger.Instance.DebugFormat("Finding item based on criteria: ({0}) on ({1})", searchCriteria, initializeOption.Identifier);

            return Create(containerItemFactory, searchCriteria, actionListener, timeout);
        }


        private IUIItem Create(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener)
        {
            IUIItem item = containerItemFactory.Get(searchCriteria, actionListener);
            if (item == null) return null;
            windowItemsMap.Add(item.Location, searchCriteria);
            return item;
        }

        private IUIItem Create(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener, TimeSpan timeout)
        {
            var recheckTime = TimeSpan.FromMilliseconds(50);
            var clock = new Clock((int) timeout.TotalMilliseconds, (int) recheckTime.TotalMilliseconds);
            Clock.Matched matched = obj => obj != null;
            Clock.Expired expired = () => null;
            WhiteLogger.Instance.DebugFormat("Create start with timeout not nullable");
            var item = (IUIItem)clock.Perform(() => containerItemFactory.Get(searchCriteria, actionListener), matched, expired);
            WhiteLogger.Instance.DebugFormat("Create stop with timeout  not nullable");
            if (item == null) return null;
            if (windowItemsMap != null)
            {
                windowItemsMap.Add(item.Location, searchCriteria);
            }
            else
                WhiteLogger.Instance.DebugFormat("No windowItemsMap object for item {0}", searchCriteria);
            return item;
        }

        public virtual void Dispose()
        {
            windowItemsMap.Save();
        }

        public virtual void Register(Window window)
        {
            window.Focus();
            LocationChanged(window);
        }

        public virtual void LocationChanged(Window window)
        {
            windowItemsMap.CurrentWindowPosition = window.Location;
        }

        public virtual IUIItem Get(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener, TimeSpan? timeout)
        {
            if (initializeOption!=null)
            {
                WhiteLogger.Instance.DebugFormat("Finding item based on criteria: ({0}) on ({1})", searchCriteria, initializeOption.Identifier);    
            }
            else
            {
                WhiteLogger.Instance.DebugFormat("Finding item based on criteria: ({0})", searchCriteria);
            }
            
            if (timeout == null)
            {
                timeout = TimeSpan.FromMilliseconds(CoreAppXmlConfiguration.Instance.SearchTimeout);
            }
            return Create(containerItemFactory, searchCriteria, actionListener, timeout);
        }

        //TODO: nullable timeot problem
        private IUIItem Create(ContainerItemFactory containerItemFactory, SearchCriteria searchCriteria, ActionListener actionListener, TimeSpan? timeout = null)
        {
            try
            {
                var timeoutIn = new TimeSpan();
            
                if (timeout == null)
                {
                    timeoutIn = TimeSpan.FromMilliseconds(CoreAppXmlConfiguration.Instance.SearchTimeout);
                }
                else
                {
                    timeoutIn = timeout.Value;
                }
                var recheckTime = TimeSpan.FromMilliseconds(50);
                var clock = new Clock((int)timeoutIn.TotalMilliseconds, (int)recheckTime.TotalMilliseconds);
                Clock.Matched matched = obj => obj != null;
                Clock.Expired expired = () => null;
                WhiteLogger.Instance.DebugFormat("Create UIItem start, timeout {0}", timeoutIn.ToString());
                var item = (IUIItem)clock.Perform(() => containerItemFactory.Get(searchCriteria, actionListener), matched, expired);
                WhiteLogger.Instance.DebugFormat("Create UIItem stop");
                if (item == null) return null;
                if (windowItemsMap != null)
                {
                    windowItemsMap.Add(item.Location, searchCriteria);
                }
                else
                    WhiteLogger.Instance.DebugFormat("No windowItemsMap for item {0}", searchCriteria);
                return item;
            }
            catch (Exception e)
            {
                throw new WhiteException("White.Core.Sessions.Get IUIItem ", e);
            }
        }
    }
}