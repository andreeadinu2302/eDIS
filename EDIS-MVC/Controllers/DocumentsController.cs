using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS_MVC.Data;
using EDIS_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NSec.Cryptography;
using Omnibasis.BigchainCSharp.Util;
using Omnibasis.BigchainCSharp.Model;
using Omnibasis.BigchainCSharp.Builders;
using System.Net;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Api;
using Newtonsoft.Json;
using SimpleBase;

namespace EDIS_MVC.Controllers
{
    [Authorize(Roles = "Organization")]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DocumentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var user_id = user.Id;
            var organization = _context.Organizations.Where(x => x.AdminUserId == user_id).FirstOrDefault();
            ViewBag.organization = organization;

            var list = await _context.Documents.Where(x => x.OrganizationId == organization.Id).ToListAsync();

            return View(list);
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var user_id = user.Id;
            var organization = _context.Organizations.Where(x => x.AdminUserId == user_id).FirstOrDefault();
            ViewBag.organization = organization;

            return View();
        }

        public async Task<IActionResult> Validate(string TransactionId)
        {
            var conn1Config = new Dictionary<string, object>();

            //config connection 1
            conn1Config.Add("baseUrl", "https://test.ipdb.io");
            BlockchainConnection conn1 = new BlockchainConnection(conn1Config);

            var conn2Config = new Dictionary<string, object>();
            var headers2 = new Dictionary<string, string>();

            //config connection 2
            conn2Config.Add("baseUrl", "http://ebcl2.ici.ro");
            BlockchainConnection conn2 = new BlockchainConnection(conn2Config);

            var conn3Config = new Dictionary<string, object>();
            var headers3 = new Dictionary<string, string>();

            //config connection 2
            conn3Config.Add("baseUrl", "http://ebcl3.ici.ro");
            BlockchainConnection conn3 = new BlockchainConnection(conn2Config);

            //add connections
            IList<BlockchainConnection> connections = new List<BlockchainConnection>();
            connections.Add(conn1);
            //connections.Add(conn2);
            //connections.Add(conn3);

            //multiple connections
            var builderWithConnections = BigchainDbConfigBuilder
                .addConnections(connections)
                .setTimeout(60000); //override default timeout of 20000 milliseconds

            await builderWithConnections.setup();

            var testTran2 = await TransactionsApi<object, object>.getTransactionByIdAsync(TransactionId);


            var signer = testTran2.Outputs[0].PublicKeys[0];
            DocumentTransaction asset = JsonConvert.DeserializeObject<DocumentTransaction>(testTran2.Asset.Data.ToString());

            var organizations = _context.Organizations.ToList();

            foreach (var o in organizations)
            {
                ViewBag.publickey1 = Utils.ByteArrayToString(o.PublicKey);
                ViewBag.publickey2 = Utils.ByteArrayToString(Base58.Bitcoin.Decode(signer).ToArray());
                if (Utils.ByteArrayToString(o.PublicKey) == Utils.ByteArrayToString(Base58.Bitcoin.Decode(signer).ToArray()))
                {
                    ViewBag.publickey1 = Utils.ByteArrayToString(o.PublicKey);
                    //Signer found
                    ViewBag.signer = o;
                    ViewBag.asset = asset;
                    return View();
                }
            }

            ViewBag.signer = "The signer of this diploma was not recognized";
            ViewBag.asset = asset;

            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DocumentType,DocumentSerial,Faculty,Title,LearningType,Average,HolderName")] Document document)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(User.Identity.Name);
                var user_id = user.Id;
                var organization = _context.Organizations.Where(x => x.AdminUserId == user_id).FirstOrDefault();
                ViewBag.organization = organization;

                var algorithm = SignatureAlgorithm.Ed25519;

                var privateKey = Key.Import(algorithm, organization.PrivateKey, KeyBlobFormat.NSecPrivateKey);
                var publicKey = PublicKey.Import(algorithm, organization.PublicKey, KeyBlobFormat.NSecPublicKey);

                //Sign the transaction to the blockchain
                var conn1Config = new Dictionary<string, object>();

                //config connection 1
                conn1Config.Add("baseUrl", "https://test.ipdb.io");
                BlockchainConnection conn1 = new BlockchainConnection(conn1Config);

                var conn2Config = new Dictionary<string, object>();
                var headers2 = new Dictionary<string, string>();

                //config connection 2
                conn2Config.Add("baseUrl", "http://ebcl2.ici.ro");
                BlockchainConnection conn2 = new BlockchainConnection(conn2Config);

                var conn3Config = new Dictionary<string, object>();
                var headers3 = new Dictionary<string, string>();

                //config connection 2
                conn3Config.Add("baseUrl", "http://ebcl3.ici.ro");
                BlockchainConnection conn3 = new BlockchainConnection(conn2Config);

                //add connections
                IList<BlockchainConnection> connections = new List<BlockchainConnection>();
                connections.Add(conn1);
                //connections.Add(conn2);
                //connections.Add(conn3);

                //multiple connections
                var builderWithConnections = BigchainDbConfigBuilder
                    .addConnections(connections)
                    .setTimeout(60000); //override default timeout of 20000 milliseconds

                await builderWithConnections.setup();

                DocumentTransaction assetData = new DocumentTransaction();
                assetData.OrganizationName = organization.Name;
                assetData.DocumentType = document.DocumentType;
                assetData.DocumentSerial = document.DocumentSerial;
                assetData.Faculty = document.Faculty;
                assetData.Title = document.Title;
                assetData.LearningType = document.LearningType;
                assetData.Average = document.Average;
                assetData.DateOfIssue = DateTime.Now;
                assetData.HolderName = document.HolderName;

                //TestMetadata metaData = new TestMetadata();
                TransactionsMetadata metaData = new TransactionsMetadata();
                string externalip = new WebClient().DownloadString("http://icanhazip.com");
                metaData.SignerIP = externalip;

                var transaction = BigchainDbTransactionBuilder<DocumentTransaction, TransactionsMetadata>
                        .init()
                        .addAssets(assetData)
                        .addMetaData(metaData)
                        .operation(Operations.CREATE)
                        .buildAndSignOnly(publicKey, privateKey);

                try
                {
                    var createTransaction = await TransactionsApi<DocumentTransaction, TransactionsMetadata>.sendTransactionAsync(transaction);
                    if (createTransaction != null && createTransaction.Data != null)
                    {
                        document.TransactionId = createTransaction.Data.Id;
                        document.OrganizationId = organization.Id;
                        _context.Add(document);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrganizationName,DocumentType,DocumentSerial,Faculty,Title,LearningType,Average")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
