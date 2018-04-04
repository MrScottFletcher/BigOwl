using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.Entity
{
    public class OwlCommandQueue
    {

        System.Threading.ReaderWriterLockSlim _lock;
        private List<OwlCommand> _commandList;

        public delegate void QueueChangedHandler(object sender);
        public delegate void QueueEmptyHandler(object sender);
        public delegate void CommandAddedHandler(object sender, OwlCommand command);

        public event QueueChangedHandler QueueChanged;
        public event QueueEmptyHandler QueueEmpty;
        public event CommandAddedHandler CommandAdded;

        public static OwlCommandQueue Instance { get; } = new OwlCommandQueue();

        public OwlCommandQueue()
        {
            _lock = new System.Threading.ReaderWriterLockSlim();
            _commandList = new List<OwlCommand>();
        }

        public void Add(OwlCommand c)
        {
            Add(c, false);
        }

        public void Add(OwlCommand c, bool insertAtFront)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                List<OwlCommand> found = _commandList.FindAll(x => x.Id == c.Id);
                if (found.Count == 0)
                    _lock.EnterWriteLock();
                try
                {
                    if (insertAtFront)
                        _commandList.Insert(0, c);
                    else
                        _commandList.Add(c);

                    FireCommandAddedEvent(c);
                }
                finally
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
            catch (Exception exAny)
            {
                //Log something?
                throw;
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }
        }

        //------------------
        private void FireCommandAddedEvent(OwlCommand c)
        {
            CommandAdded?.Invoke(this, c);
            FireQueueChangedEvent();
        }
        private void FireQueueChangedEvent()
        {
            QueueChanged?.Invoke(this);
        }
        private void FireQueueEmptyEvent()
        {
            QueueEmpty?.Invoke(this);
        }
        //------------------

        public OwlCommand GetNext()
        {
            return GetNext(true);
        }

        public OwlCommand PeekNext()
        {
            return GetNext(false);
        }

        protected OwlCommand GetNext(bool remove)
        {
            OwlCommand next = null;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_commandList.Count > 0)
                {
                    next = _commandList.First();
                    if (remove)
                    {
                        _lock.EnterWriteLock();
                        try
                        {
                            _commandList.Remove(next);
                            FireQueueChangedEvent();

                            if (_commandList.Count == 0)
                                FireQueueEmptyEvent();
                        }
                        finally
                        {
                            if (_lock.IsWriteLockHeld)
                                _lock.ExitWriteLock();
                        }
                    }
                }
            }
            catch (Exception exAny)
            {
                //Log something?
                throw;
            }
            finally
            {
                if (_lock.IsUpgradeableReadLockHeld)
                    _lock.ExitUpgradeableReadLock();
            }

            return next;
        }

        public int Remove(OwlCommand c)
        {
            return Remove(c.Id);
        }

        public int Remove(Guid id)
        {
            int countRemoved = 0;
            _lock.EnterWriteLock();
            try
            {
                countRemoved = _commandList.RemoveAll(x => x.Id == id);

                FireQueueChangedEvent();

                if (_commandList.Count == 0)
                {
                    FireQueueEmptyEvent();
                }
            }
            catch (Exception exAny)
            {
                //Log something?
                throw;
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
            return countRemoved;
        }



        public int RemoveAllForSource(string sourceAppId)
        {
            int countRemoved = 0;
            _lock.EnterWriteLock();
            try
            {
                countRemoved = _commandList.RemoveAll(x => x.SourceAppId == sourceAppId);
                FireQueueChangedEvent();
            }
            catch (Exception exAny)
            {
                //Log something?
                throw;
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
            return countRemoved;
        }

        public int RemoveAll()
        {
            int countRemoved = 0;
            _lock.EnterWriteLock();
            try
            {
                countRemoved = _commandList.Count();
                _commandList.Clear();
                FireQueueChangedEvent();
                FireQueueEmptyEvent();
            }
            catch (Exception exAny)
            {
                //Log something?
                throw;
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
            return countRemoved;
        }
    }
}