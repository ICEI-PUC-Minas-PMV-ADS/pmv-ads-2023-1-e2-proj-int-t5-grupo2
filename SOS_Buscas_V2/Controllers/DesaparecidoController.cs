﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using SOS_Buscas_V2.Helper;
using SOS_Buscas_V2.Models;
using SOS_Buscas_V2.Repositorio;
using System.Collections.Generic;

namespace SOS_Buscas_V2.Controllers
{
    public class DesaparecidoController : Controller
    {
        //----------------------------------------------------------------------
        // Construtor para injeção de dependencia das Interfaces IDesaparecido e ISessao
        private readonly IDesaparecido _iDesaparecido;
        private readonly ISessao _iSessao;
        private readonly string _caminhoImagem;

        private string _IdDesaparecido;
        
        
        public DesaparecidoController(IDesaparecido iDesaparecido, ISessao iSessao, IWebHostEnvironment caminhoImagem)
        {
            _iDesaparecido = iDesaparecido;
            _iSessao = iSessao;
            _caminhoImagem = caminhoImagem.WebRootPath;
            
        }

        //----------------------------------------------------------------------
        //Metodo para chamar a pagina Index
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ApagarPage(DesaparecidoModel desaparecido)
        {
            if (_iSessao.BuscarSessao() == null) return Json(new { Msg = "Você precisa estar logado" });

            DesaparecidoModel desaparecidoDB = _iDesaparecido.ListarPorId(desaparecido.Id);
            return View(desaparecidoDB);
        }


        public IActionResult EditarPage(DesaparecidoModel desaparecido)
        {
            if (_iSessao.BuscarSessao() == null) return Json(new { Msg = "Você precisa estar logado" });

            DesaparecidoModel desaparecidoDB = _iDesaparecido.ListarPorId(desaparecido.Id);
            return View(desaparecidoDB);
        }

        //----------------------------------------------------------------------
        //Metodo para editar informações do desaparecido cadastrado

        
        [HttpPost]
        public IActionResult Editar(int id, DesaparecidoModel desaparecido)
        {

            DesaparecidoModel desaparecidoDBid = _iDesaparecido.ListarPorId(id);


            UsuarioModel usuario = _iSessao.BuscarSessao();

            string EmailUsuario = usuario.Email;

            if (EmailUsuario == desaparecidoDBid.EmailUsuario)
            {
                DesaparecidoModel desaparecidoDB = _iDesaparecido.Editar(desaparecido);

                if (desaparecidoDB != null)
                {
                    return Json(new { Msg = "correto" });
                }
                return Json(new { Msg = "erro, esse desaparecido não existe" });
            }
            return Json(new { Msg = "erro, você não cadastrou o desaparecido" });

            


        }

        //----------------------------------------------------------------------
        //Metodo para chamar a pagina de cadastro de desaparecidos


        public IActionResult CadastrarPage()
        {
            if (_iSessao.BuscarSessao() == null) return Json(new { Msg = "Você precisa estar logado" });
            return View();
        }

        

        //----------------------------------------------------------------------
        //Metodo para verificar se o desaparecido já foi cadastrado e caso não tenha sido cadastrar o mesmo

        [HttpPost]
        public IActionResult Cadastrar(DesaparecidoModel desaparecido, IFormFile foto)
        {

            //------------------------------------------------------------------
            //Gera o nome da imagem do desaparecido e cadastra esse nome no banco

            string CaminhoDaImagem = _caminhoImagem + "\\Imagens\\";
            string nomeImagem = Guid.NewGuid().ToString() + "_" + foto.FileName;

            if (!Directory.Exists(CaminhoDaImagem))
            {
                Directory.CreateDirectory(CaminhoDaImagem);
            }

            using (var stream = System.IO.File.Create(CaminhoDaImagem + nomeImagem))
            {
                foto.CopyToAsync(stream);
            }

            desaparecido.CaminhoImagem = nomeImagem;

            //------------------------------------------------------------------
            // Informa para a DesaparecidoModel qual é o email do usuário que esta cadastrando o desaparecido
            UsuarioModel usuario = _iSessao.BuscarSessao();

            string EmailUsuario = usuario.Email;

            desaparecido.EmailUsuario = EmailUsuario;
            //------------------------------------------------------------------
            //Verifica e cadastra o desaparecido no banco

            

            List<DesaparecidoModel> desaparecidos = _iDesaparecido.Listar();

            if (desaparecidos != null && desaparecidos.Any())
            {
                foreach (DesaparecidoModel missing in desaparecidos)  //Verifica se o desaparecido já foi cadastrado
                {
                    if (missing.NomeCompleto == desaparecido.NomeCompleto )
                    {
                        return Json(new { Msg = "esse usuario já existe" });
                    }
                }
                DesaparecidoModel teste = _iDesaparecido.Criar(desaparecido);   //Cadastra o desaprecido no banco
                
                return View("Index");
            }

            DesaparecidoModel teste2 = _iDesaparecido.Criar(desaparecido);    //Cadastrar o desaparecido no banco
            
            return View("Index");
        }



        //------------------------------------------------------------------
        //Retorna os dados dos desaparecidos para a montagem dos cards


        private List<DesaparecidoModel> _desaparecido;     //Variavel que recebe uma lista com os dados dos desaparecidos
        public IActionResult DesaparecidosPage()
        {
            List<DesaparecidoModel> desaparecidos = _iDesaparecido.Listar();
            _desaparecido = desaparecidos;

            foreach (DesaparecidoModel desaparecido in _desaparecido)      //Laço de repetição que cria os dados das colunas
            {
                List<DesaparecidoModel> dadosDesaparecido = _iDesaparecido.Listar();
                return View(dadosDesaparecido);
            }

            return View();
        }

        public IActionResult Apagar(int id)
        {

            DesaparecidoModel desaparecido = _iDesaparecido.ListarPorId(id);


            UsuarioModel usuario = _iSessao.BuscarSessao();

            string EmailUsuario = usuario.Email;

            if(EmailUsuario == desaparecido.EmailUsuario)
            {
                bool desaparecidoDB = _iDesaparecido.Apagar(desaparecido);

                if (desaparecidoDB == true)
                {
                    return Json(new { Msg = "Apagado com sucesso" });
                }
                return Json(new { Msg = "erro, esse desaparecido não existe" });
            }
            return Json(new { Msg = "erro, você não cadastrou o desaparecido" });


        }

       public IActionResult AlterarAvistamentoPage(int id)
        {
            if (_iSessao.BuscarSessao() == null) return Json(new { Msg = "Você precisa estar logado" });

            DesaparecidoModel desaparecidoDB = _iDesaparecido.ListarPorId(id);
            return View(desaparecidoDB);
        }

        public IActionResult AlterarAvistamento(DesaparecidoModel desaparecido)
        {
            DesaparecidoModel desaparecidoDB = _iDesaparecido.Editar(desaparecido);

            if (desaparecidoDB != null)
            {
                return Json(new { Msg = "ecereto" });
            }
            return Json(new { Msg = "erro" });
        }





       


        

    }
}
