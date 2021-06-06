using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS_MVC.Data;
using EDIS_MVC.Models;
using Omnibasis.BigchainCSharp.Model;
using Omnibasis.BigchainCSharp.Builders;
using NSec.Cryptography;
using System.Net;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Api;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS_MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public OrganizationsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        // GET: Organizations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Organizations.ToListAsync());
        }

        // GET: Organizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // GET: Organizations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Logo,Domain")] Organization organization)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(organization.Logo.FileName);
                string extension = Path.GetExtension(organization.Logo.FileName);
                organization.LogoName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                string path = Path.Combine(wwwRootPath + "/logos/", organization.LogoName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await organization.Logo.CopyToAsync(fileStream);
                }

                //PublicKey / PrivateKey / TransactionId
                var algorithm = SignatureAlgorithm.Ed25519;

                var privateKey = Key.Create(algorithm, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
                var publicKey = privateKey.PublicKey;

                organization.PublicKey = publicKey.Export(KeyBlobFormat.NSecPublicKey);
                organization.PrivateKey = privateKey.Export(KeyBlobFormat.NSecPrivateKey);

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

                OrganizationTransaction assetData = new OrganizationTransaction();
                assetData.OrganizationName = organization.Name;
                assetData.DateOfCreation = DateTime.Now;
                assetData.PublicKey = publicKey.Export(KeyBlobFormat.NSecPublicKey);

                //TestMetadata metaData = new TestMetadata();
                TransactionsMetadata metaData = new TransactionsMetadata();
                string externalip = new WebClient().DownloadString("http://icanhazip.com");
                metaData.SignerIP = externalip;

                var transaction = BigchainDbTransactionBuilder<OrganizationTransaction, TransactionsMetadata>
                        .init()
                        .addAssets(assetData)
                        .addMetaData(metaData)
                        .operation(Operations.CREATE)
                        .buildAndSignOnly(publicKey, privateKey);

                var createTransaction = await TransactionsApi<OrganizationTransaction, TransactionsMetadata>.sendTransactionAsync(transaction);

                if (createTransaction != null && createTransaction.Data != null)
                {
                    //Create the admin user of this organization
                    //Create the admin of the application
                    var user = new IdentityUser();
                    user.UserName = "admin@" + organization.Domain;
                    user.Email = "admin@" + organization.Domain;
                    user.EmailConfirmed = true;
                    string userPWD = "1qaz!QAZ";
                    IdentityResult chkUser = await _userManager.CreateAsync(user, userPWD);
                    await _userManager.AddToRoleAsync(user, "Organization");
                    var written_user = await _userManager.FindByEmailAsync(user.Email);
                    organization.AdminUserId = written_user.Id;

                    organization.TransactionId = createTransaction.Data.Id;
                    _context.Add(organization);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            return View(organization);
        }

        // GET: Organizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Logo,PublicKey,PrivateKey,TransactionId")] Organization organization)
        {
            if (id != organization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.Id))
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
            return View(organization);
        }

        // GET: Organizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }
    }
}
