// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Foo.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Example
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo(Bar bar, IZar zar, IEnumerable<Emu> emus, Generic<TypeParam> generic)
        {
        }
    }

    public class Bar
    {
        public Bar(IMyFactory factory)
        {
        }
    }

    public interface IZar {}

    public class Zar1 : IZar {}

    public class Zar2 : IZar {}

    public interface IMyFactory
    {
        Blah CreateBlah();
        Bloh CreateBloh();
    }

    public class Emu {}

    public class Blah {}

    public class Bloh {}

    public class Generic<T> {}

    public class TypeParam {}
}