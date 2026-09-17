using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;

namespace Revision_App.Services
{
    public static class SupabaseService
    {
        // supabase url and api key
        private static readonly string SupabaseUrl =
            "https://mgwychahyxnixcafwebo.supabase.co";

        private static readonly string SupabaseKey =
            "sb_publishable_nkTGlKMUHrEEuqI3RK4H_w_Wmlcy2QO";

       
        public static Supabase.Client? Client { get; private set; }

        public static async Task InitializeAsync()
        {
            Client = new Supabase.Client(
                SupabaseUrl,
                SupabaseKey
            );

            await Client.InitializeAsync();
        }
    }
}
