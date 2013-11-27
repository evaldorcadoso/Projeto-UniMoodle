<?php

//faz o include da página de configuração do moodle
include "../moodle/config.php";


$link = mysql_connect($CFG->dbhost,$CFG->dbuser,$CFG->dbpass) or die ('Ops! Sem conexão com o servidor');
mysql_select_db($CFG->dbname) or die ('Ops! Banco de dados não encontrado');
mysql_set_charset('utf8',$link);

/*Configurações do local dos arquivos*/

//caminho completo até a pasta filedir/
$GLOBALS['caminhoCompleto']  = '';

//caminho temporario até a pasta files/
$GLOBALS['caminhoTemporario']  = '';
//local do server com a pasta temporaria files/
$GLOBALS['localServer']='';


function verificaToken($token)
{
	$rs = mysql_query("SELECT id,token FROM mdl_external_tokens WHERE token='$token'");
		$resultado = array();
		if(mysql_num_rows($rs)>0)
		{
			return true;
		}
		else
		{
			return false;
		}
}
 
?>