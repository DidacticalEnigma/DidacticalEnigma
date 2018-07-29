/**
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 Johan Larsson
 * All rights reserved. 
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software 
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
// ReSharper disable StaticMemberInGenericType
namespace DidacticalEnigma
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> with support for AddRange and RemoveRange
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    public class ObservableBatchCollection<T> : ObservableCollection<T>
    {
        private static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Count));
        private static readonly PropertyChangedEventArgs IndexerPropertyChangedEventArgs = new PropertyChangedEventArgs("Item[]");
        private static readonly NotifyCollectionChangedEventArgs NotifyCollectionResetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableBatchCollection{T}"/> class.
        /// It is empty and has default initial capacity.
        /// </summary>
        public ObservableBatchCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableBatchCollection{T}"/> class.
        /// It contains elements copied from the specified list
        /// </summary>
        /// <param name="collection">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public ObservableBatchCollection(IList<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableBatchCollection{T}"/> class.
        /// It contains the elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public ObservableBatchCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Add a range of elements. If the count is greater than 1 one reset event is raised when done.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            this.CheckReentrancy();
            var count = 0;
            var added = default(T);
            foreach (var item in items)
            {
                if (count > 0)
                {
                    this.Items.Add(item);
                }
                else
                {
                    added = item;
                    this.Items.Add(item);
                }

                count++;
            }

            if (count == 0)
            {
                return;
            }

            if (count == 1)
            {
                this.OnPropertyChanged(CountPropertyChangedEventArgs);
                this.OnPropertyChanged(IndexerPropertyChangedEventArgs);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, this.Items.Count - 1));
            }
            else
            {
                this.RaiseReset();
            }
        }

        /// <summary>
        /// Remove a range of elements. If the count is greater than 1 one reset event is raised when done.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void RemoveRange(IEnumerable<T> items)
        {
            this.CheckReentrancy();
            var count = 0;
            var index = -1;
            var removed = default(T);
            foreach (var item in items)
            {
                if (count > 0)
                {
                    this.Items.Remove(item);
                }
                else
                {
                    removed = item;
                    index = this.Items.IndexOf(item);
                    this.Items.RemoveAt(index);
                }

                count++;
            }

            if (count == 0)
            {
                return;
            }

            if (count == 1)
            {
                this.OnPropertyChanged(CountPropertyChangedEventArgs);
                this.OnPropertyChanged(IndexerPropertyChangedEventArgs);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
            }
            else
            {
                this.RaiseReset();
            }
        }

        private void RaiseReset()
        {
            this.OnPropertyChanged(CountPropertyChangedEventArgs);
            this.OnPropertyChanged(IndexerPropertyChangedEventArgs);
            this.OnCollectionChanged(NotifyCollectionResetEventArgs);
        }
    }
}