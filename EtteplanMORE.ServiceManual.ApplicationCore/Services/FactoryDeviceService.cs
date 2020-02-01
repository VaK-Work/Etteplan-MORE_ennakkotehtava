using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EtteplanMORE.ServiceManual.ApplicationCore.Entities;
using EtteplanMORE.ServiceManual.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EtteplanMORE.ServiceManual.ApplicationCore.Services
{
    public class FactoryDeviceService : IFactoryDeviceService
    {
        //Yhteysteksti, jossa on tietokannan yhteystieto
        readonly static string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\Työnhakutesti\\servicemanual-csharp-master\\EtteplanMORE.ServiceManual.ApplicationCore\\HuoltotehtDB.mdf;Integrated Security=True";
        //Luodaan muuttuja SQL-tiedoston HuoltotehtDB.mdf yhteydenottoa varten.
        //connectionString-muuttujaa käytetään tässä 
        private SqlConnection con = new SqlConnection(connectionString);
        

        //Task hakee kaikki huoltotehtävät tietokannasta, järjestää ne kriittisyyden ja kirjausajan mukaan, pakkaa ne FactoryDevice-tyyppiseen listaan, ja palauttaa sen.
        public async Task<IEnumerable<FactoryDevice>> GetAll()
        {

            //Avataan yhteys tietokantaan
            await con.OpenAsync();
            
            //Luodaan kutsu tietokantaan. Haetaan kaikki tiedot, ja järjestetään ne ensin kriittisyyden, ja sitten kirjausajan mukaan
            SqlCommand getall = new SqlCommand("SELECT * FROM Huoltotehtavat ORDER BY Kriittisyys, Kirjausaika DESC", con);
            //Aloitetaan lukemaan saatua hakutulosta, ja syötetään saadut tiedot yksi tulos kerrallaan listaan "getallresult".
            using (SqlDataReader reader = getall.ExecuteReader())
            {
                //alustetaan lista
                List<FactoryDevice> getallresult = new List<FactoryDevice>();
                while (reader.Read())
                {
                    //jokaiselle readerin tulokselle luodaan FactoryDevice-objekti "fd", ja syötetään luetut tiedot sinne. Sitten syötetään fd listaan.
                    FactoryDevice fd = new FactoryDevice();
                    fd.Id = Convert.ToInt32(reader.GetValue(0));
                    fd.Kohde = reader.GetValue(1).ToString();
                    fd.Kirjausaika = Convert.ToDateTime(reader.GetValue(2));
                    fd.Kuvaus = reader.GetValue(3).ToString();
                    fd.Kriittisyys = Convert.ToInt32(reader.GetValue(4));                   
                    fd.Tila = Convert.ToBoolean(reader.GetValue(5));
                    getallresult.Add(fd);
                }
                //Suljetaan yhteys ja palautetaan lista
                con.Close();
                con.Dispose();
                return await Task.FromResult(getallresult);
            }
            

        }

        //Haetaan taulusta kaikki kohde-parametria vastaavat huoltotehtävät
        public async Task<IEnumerable<FactoryDevice>> GetAllKohde(string kohde)
        {     
            //Avataan yhteys
            await con.OpenAsync();

            //SQL-komennolla haetaan kaikki huoltotehtävät joiden "Kohde" tietueet sisältävät hakuparametrin sisältävän tekstin. Tulokset järjestetään myös.
            SqlCommand getall = new SqlCommand("SELECT * FROM Huoltotehtavat WHERE Kohde LIKE '%'+ @k +'%' ORDER BY Kriittisyys, Kirjausaika DESC", con);
            //Syötetään sql-parametri @k hakuun, ja annetaan sille kohde
            getall.Parameters.Add(new SqlParameter("k", kohde));
            using (SqlDataReader reader = getall.ExecuteReader())
            {
                List<FactoryDevice> getallresult = new List<FactoryDevice>();
                while (reader.Read())
                {                    
                    FactoryDevice fd = new FactoryDevice();
                    fd.Id = Convert.ToInt32(reader.GetValue(0));
                    fd.Kohde = reader.GetValue(1).ToString();
                    fd.Kirjausaika = Convert.ToDateTime(reader.GetValue(2));
                    fd.Kuvaus = reader.GetValue(3).ToString();
                    fd.Kriittisyys = Convert.ToInt32(reader.GetValue(4));
                    fd.Tila = Convert.ToBoolean(reader.GetValue(5));
                    getallresult.Add(fd);
                }
                con.Close();
                con.Dispose();
                return await Task.FromResult(getallresult);
            }


        }
        //Syötetään uusi huoltotehtävä tietokantaan, ja palautetaan luodun tehtävän uusi id takaisin. Parametrinä tulee kutsuvan laitteen bodysta tarvittavat tiedot JSON muodossa FactoryDevice-objektina
        public async Task<int> InsertNew([FromBody]FactoryDevice fd)
        {
            //avataan yhteys
            await con.OpenAsync();

            //Luodaan SQL-komento, jolla syötetään tulleet tiedot. SCOPE_IDENTITY() palauttaa luodun kohteen Id:n
            SqlCommand insertnewtask = new SqlCommand("INSERT INTO Huoltotehtavat VALUES (@Kohde, @Kirjausaika, @Kuvaus, @Kriittisyys, @Tila); SELECT SCOPE_IDENTITY();", con);
            insertnewtask.Parameters.Add(new SqlParameter("Kohde",fd.Kohde));
            insertnewtask.Parameters.Add(new SqlParameter("Kirjausaika",fd.Kirjausaika));
            insertnewtask.Parameters.Add(new SqlParameter("Kuvaus",fd.Kuvaus));
            insertnewtask.Parameters.Add(new SqlParameter("Kriittisyys",fd.Kriittisyys));
            insertnewtask.Parameters.Add(new SqlParameter("Tila",fd.Tila));

            //Otetaan luodun kohteen Id talteen, suljetaan yhteys, ja palautetaan Id.
            int newid = Convert.ToInt32(insertnewtask.ExecuteScalar());
            con.Close();
            con.Dispose();
            return await Task.FromResult(newid);
        }

        //Haetaan Id:n perusteella huoltotehtävä tietokannasta
        public async Task<FactoryDevice> Get(int id)
        {
            //avataan yhteys
            await con.OpenAsync();

            //SQL-komennolla haetaan kaikki huoltotehtävät, joilla on parametria vastaava Id. Tuloksia tulee vain yksi, koska Id:t ovat uniikkeja.
            SqlCommand getID = new SqlCommand("SELECT * FROM Huoltotehtavat WHERE Id = @id", con);
            getID.Parameters.Add(new SqlParameter("id", id));

            //Luetaan saatu tulos result-nimiseen FactoryDevice-tyyppiseen muuttujaan.
            using (SqlDataReader reader = getID.ExecuteReader())
            {
                FactoryDevice result = new FactoryDevice();
                while (reader.Read())
                {                 
                    result.Id = Convert.ToInt32(reader.GetValue(0));
                    result.Kohde = reader.GetValue(1).ToString();
                    result.Kirjausaika = Convert.ToDateTime(reader.GetValue(2));
                    result.Kuvaus = reader.GetValue(3).ToString();
                    result.Kriittisyys = Convert.ToInt32(reader.GetValue(4));
                    result.Tila = Convert.ToBoolean(reader.GetValue(5));
                }
                //suljetaan yhteys ja palautetaan saatu tulos
                con.Close();
                con.Dispose();
                return await Task.FromResult(result);
            }
        }
        //Poistetaan Id:tä vastaava huoltotehtävä tietokannasta
        public async Task<string[]> Delete(int id)
        {
            //avataan yhteys
            await con.OpenAsync();

            //Sql-komento, jolla poistetaan kaikki huoltotehtävät, joiden Id vastaa parametrin Id:tä. Poistoja tehdään vain yksi, koska Id:t ovat uniikkeja.
            SqlCommand DeleteByID = new SqlCommand("DELETE FROM Huoltotehtavat WHERE Id = @id", con);
            DeleteByID.Parameters.Add(new SqlParameter("id", id));

            try
            {
                //Yritetään poistoa, ja otetaan talteen muuttuneiden rivien määrä, ja suljetaan yhteys
                int affectedrows = DeleteByID.ExecuteNonQuery();
                con.Close();
                con.Dispose();
                //Jos yhtään riviä ei muuttunut, palautetaan tieto siitä. Jos rivejä muuttui, kerrotaan että parametrin Id:llä on poistettu huoltotehtävä.
                if (affectedrows <= 0)
                {
                    return await Task.FromResult(new string[] { "Tällä ID:llä olevaa huoltotehtävää ei ole olemassa.", "0" });
                }
                else
                {
                    return await Task.FromResult(new string[] { "Tehtävä ID = " + id + " on poistettu","1" });
                }
                
                
            }catch(SqlException ex)
            {
                //Virheentarkistus, jos jokin menee pieleen. Palautetaan viesti asiasta.
                con.Close();
                con.Dispose();
                return await Task.FromResult(new string[] { "Tehtävä ID = " + id + " poisto epäonnistui: " + ex.Message ,"0"});
            }
 
        }

        //Päivitetään huoltotehtävä tietokannasta. Parametrina tulee muutettavan tehtävän Id, ja muutettavat tiedot FactoryDevice-muuttujana.
        public async Task<string[]> Update(int id, FactoryDevice fd)
        {
            //avataan yhteys, ja tarkistetaan että onko FactoryDevice fd tyhjä. Jos se on, lopetetaan päivitys ja palautetaan viesti tyhjästä pyynnöstä
            await con.OpenAsync();
            if(String.IsNullOrEmpty(fd.Kohde) && String.IsNullOrEmpty(fd.Kuvaus) && fd.Kriittisyys == 0 && fd.Tila == null)
            {
                return await Task.FromResult(new string[] { "Virhe tehtävän päivittämisessä. Lähetit tyhjän pyynnön!", "0" });
            }

            //Näillä booleaneilla tarkistetaan mitkä tiedot on päivitetty. Näitä käytetään apuna Sql-lauseen luomisessa.
            bool isKohde = false;
            bool isKuvaus = false;
            bool isKriitt = false;
            bool isTila = false;

            //Aloitetaan Sql-lauseen luomista. Alustetaan sqlcom-muuttuja lauseen pohjalla.
            //Lauseeseen lisätään päivitettävät tiedot sen perusteella, että mitkä niistä on syötetty parametreinä tähän funktioon. Nämä tarkistetaan yksi kerrallaan.
            string sqlcom = "UPDATE Huoltotehtavat SET WHERE Id = @id";


            //Aloitetaan tarkistamalla fd.Kohde. Jos se sisältää tekstiä, lisätään sqlcom-muuttujaan uusi muutettava parametri sanojen SET ja WHERE väliin.
            //Samalla ilmoitetaan, että isKohde on true, eli fd-kohde sisältää tekstiä.
            if (!String.IsNullOrEmpty(fd.Kohde))
            {
                sqlcom = sqlcom.Insert(26, "Kohde = @kohde ");
                isKohde = true;
            }

            //Sitten tarkistetaan fd.Kuvaus. Jos se sisältää tekstiä, aloitetaan Kuvauksen lisäämistä sqlcomiin. Ensin tarkistetaan, että onko sqlcom-muuttujaan syötetty Kohteesta tekstiä käyttämällä isKohde-muuttujaa.
            //Riippuen tilanteesta, syötetään Kuvauksen parametri joko pilkun kanssa tai ilman.
            if (!String.IsNullOrEmpty(fd.Kuvaus))
            {
                if (isKohde)
                {
                    sqlcom = sqlcom.Insert(26, "Kuvaus = @kuvaus, ");
                }
                else
                {
                    sqlcom = sqlcom.Insert(26, "Kuvaus = @kuvaus ");
                }
                isKuvaus = true;
            }

            //tarkistetaan onko fd.Kriittisyys-muuttujassa arvoa, ja että onko se sopiva arvo. Jos on, aloitetaan sen syöttö, ja tarkistetaan samalla tavalla kuin edellisessä lauseessa
            //että onko siellä edellistä parametrisyötettä olemassa sqlcom-muuttujassa, ja sen mukaan syötetään Kriittisyyden parametri.
            if(fd.Kriittisyys <= 3 && fd.Kriittisyys >= 1)
            {
                if(isKohde || isKuvaus)
                {
                    sqlcom = sqlcom.Insert(26, "Kriittisyys = @kriitt, ");
                }
                else
                {
                    sqlcom = sqlcom.Insert(26, "Kriittisyys = @kriitt ");
                }
                isKriitt = true;
            }

            //Vielä yhden kerran fd.Tila-muuttujalle
            if(fd.Tila != null)
            {
                if (isKohde || isKuvaus || isKriitt)
                {
                    sqlcom = sqlcom.Insert(26, "Tila = @tila, ");
                }
                else
                {
                    sqlcom = sqlcom.Insert(26, "Tila = @tila ");
                }
                isTila = true;
            }

            //Nyt kun on sql-komento kirjoitettu tekstiksi, luodaan siitä SqlCommand-tyyppinen muuttuja UpdateByID 
            SqlCommand UpdateByID = new SqlCommand(sqlcom, con);

            //Lisätään luodut parametrit, jos niitä vastaavat booleanit ovat totta.
            UpdateByID.Parameters.Add(new SqlParameter("id", id));
            if (isKohde)
            {
                UpdateByID.Parameters.Add(new SqlParameter("kohde", fd.Kohde));
            }
            if (isKuvaus)
            {
                UpdateByID.Parameters.Add(new SqlParameter("kuvaus",fd.Kuvaus));
            }
            if(isKriitt)
            {
                UpdateByID.Parameters.Add(new SqlParameter("kriitt",fd.Kriittisyys));
            }
            if (isTila)
            {
                UpdateByID.Parameters.Add(new SqlParameter("tila", Convert.ToInt32(fd.Tila)));
            }


            try
            {
                //Lähetetään komento tietokantaan, ja otetaan talteen muuttuneiden rivien määrä, ja suljetaan yhteys.
                int affectedrows = UpdateByID.ExecuteNonQuery();
                con.Close();
                con.Dispose();
                //jos muuttuneita rivejä ei ole, kerrotaan että päivitystä ei tehty, ja palautetaan viesti, ja tieto epäonnistumisesta booleanina
                //jos on muuttuneita rivejä, ilmoitetaan siitä samalla tavalla, ja lähetetään tieto onnistumisesta booleanina
                if (affectedrows <= 0)
                {
                    return await Task.FromResult(new string[] { "Päivitystä ei suoritettu, koska tehtävää ID:llä " + id + " ei ole olemassa", "0" });
                }
                else
                {
                    return await Task.FromResult(new string[] { "Tehtävä päivitetty!", "1" });
                }

            }
            catch (SqlException ex)
            { //virheentarkistus, lähetetään virheviesti ja epäonnistumisen tieto vastauksena.
                con.Close();
                con.Dispose();
                return await Task.FromResult(new string[] { "Virhe tehtävän päivittämisessä. " + ex ,"0"});
            }

        }
    }
}