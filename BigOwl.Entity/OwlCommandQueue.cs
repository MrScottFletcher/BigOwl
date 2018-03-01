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
                    _lock.EnterWriteLock();
                    try
                    {
                        _commandList.Remove(next);
                    }
                    finally
                    {
                        if (_lock.IsWriteLockHeld)
                            _lock.ExitWriteLock();
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