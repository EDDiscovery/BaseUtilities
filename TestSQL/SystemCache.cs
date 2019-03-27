using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    public static class SystemCache
    {
        // may return null if not found
        // by design, it keeps on trying.  Rob thought about caching the misses but the problem is, this is done at start up
        // the system db may not be full at that point.  So a restart would be required to clear the misses..
        // difficult

        #region Public Interface

        public static ISystem FindSystem(long edsmid)
        {
            return FindSystem(new SystemClass(edsmid));
        }

        public static ISystem FindSystem(string name)
        {
            return FindSystem(new SystemClass(name));
        }

        public static ISystem FindSystem(ISystem find)
        {
            lock (systemsByEdsmId)          // Rob seen instances of it being locked together in multiple star distance threads, we need to serialise the whole thing
            {                               // Concurrent dictionary no good, they could both be about to add the same thing at the same time and pass the contains test.
                ISystem orgsys = find;

                List<ISystem> foundlist = new List<ISystem>();

                if (find.EDSMID > 0 && systemsByEdsmId.ContainsKey(find.EDSMID))        // add to list
                {
                    ISystem s = systemsByEdsmId[find.EDSMID];
                    foundlist.Add(s);
                }

                if (systemsByName.ContainsKey(find.Name))            // and all names cached
                {
                    List<ISystem> s = systemsByName[find.Name];
                    foundlist.AddRange(s);
                }

                ISystem found = null;

                if (find.HasCoordinate && foundlist.Count > 0)           // if sys has a co-ord, find the best match within 0.5 ly
                    found = NearestTo(foundlist, find, 0.5);

                if (found == null && foundlist.Count == 1 && !find.HasCoordinate) // if we did not find one, but we have only 1 candidate, use it.
                    found = foundlist[0];

                if (found == null)                                    // nope, no cache, so use the db
                {
                    //System.Diagnostics.Debug.WriteLine("Look up from DB " + sys.name + " " + sys.id_edsm);

                    if (find.EDSMID > 0)        // if we have an ID, look it up
                        found = DB.SystemsDB.FindStar(find.EDSMID);

                    if (found == null && find.Name.HasChars())      // if not found by has a name
                        found = DB.SystemsDB.FindStar(find.Name);   // find by name, no wildcards

                    if (found != null && find.Name.HasChars())      // if we found using the edsmid or a name, use that name in preference - the edsm data may be out of date..
                        found.Name = find.Name;

                    if (found == null && find.HasCoordinate)        // finally, not found, but we have a co-ord, find it from the db  by distance
                        found = DB.SystemsDB.GetSystemByPosition(find.X, find.Y, find.Z);

                    if (found != null)                              // if we have a good db, go for it
                    {
                        if (find.HasCoordinate)                     // if find has co-ordinate, it may be more up to date than the DB, so use it
                        {
                            found.X = find.X; found.Y = find.Y; found.Z = find.Z;
                        }

                        if (systemsByName.ContainsKey(orgsys.Name))   // so, if name database already has name
                            systemsByName[orgsys.Name].Remove(orgsys);  // and remove the ISystem if present on that orgsys

                        AddToCache(found);

                        //System.Diagnostics.Trace.WriteLine($"DB found {found.name} {found.id_edsm} sysid {found.id_edsm}");

                        return found;
                    }
                    else
                    {
                        //System.Diagnostics.Trace.WriteLine($"DB NOT found {find.name} {find.id_edsm} ");
                        return null;
                    }
                }
                else
                {                                               // FROM CACHE
                    //System.Diagnostics.Trace.WriteLine($"Cached reference to {found.name} {found.id_edsm}");
                    return found;       // no need for extra work.
                }
            }
        }

        // use the DB but cache the returns for future use
        static public List<ISystem> FindSystemWildcard(string name)
        {
            var list = DB.SystemsDB.FindStarWildcard(name);
            if (list != null)
            {
                foreach (var x in list)
                    AddToCache(x);
            }

            return list;
        }

        // use the DB but cache the returns for future use
        public static void GetSystemListBySqDistancesFrom(BaseUtils.SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                                    int maxitems,
                                                    double mindist, double maxdist, bool spherical)
        {
            DB.SystemsDB.GetSystemListBySqDistancesFrom(distlist, x, y, z, maxitems, mindist, maxdist, spherical, (s) => AddToCache(s));
        }

        // use the DB but cache the returns for future use
        public static ISystem FindNearestSystemTo(double x, double y, double z, double maxdistance = 1000)
        {
            ISystem s = DB.SystemsDB.FindNearestSystemTo(x, y, z, maxdistance);
            if (s != null)
                AddToCache(s);
            return s;
        }

        // use the DB but cache the returns for future use
        public static ISystem GetSystemByPosition(double x, double y, double z)
        {
            ISystem s = DB.SystemsDB.GetSystemByPosition(x, y, z);
            if (s != null)
                AddToCache(s);
            return s;
        }

        #endregion


        #region Helpers

        static private void AddToCache(ISystem found, ISystem orgsys = null)
        {
            if (found.EDSMID > 0)
                systemsByEdsmId[found.EDSMID] = found;  // must be definition the best ID found.. and if the update date of sys is better, its now been updated

            if (systemsByName.ContainsKey(found.Name))
            {
                if ( !systemsByName[found.Name].Contains(found))
                    systemsByName[found.Name].Add(found);   // add to list..
            }
            else
                systemsByName[found.Name] = new List<ISystem> { found }; // or make list
        }

        static private ISystem NearestTo(List<ISystem> list, ISystem comparesystem, double mindist)
        {
            ISystem nearest = null;

            foreach (ISystem isys in list)
            {
                if (isys.HasCoordinate)
                {
                    double dist = isys.Distance(comparesystem);

                    if (dist < mindist)
                    {
                        mindist = dist;
                        nearest = isys;
                    }
                }
            }

            return nearest;
        }

        private static Dictionary<long, ISystem> systemsByEdsmId = new Dictionary<long, ISystem>();
        private static Dictionary<string, List<ISystem>> systemsByName = new Dictionary<string, List<ISystem>>(StringComparer.InvariantCultureIgnoreCase);

        #endregion
    }
}

