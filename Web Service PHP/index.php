<?php
date_default_timezone_set('America/Sao_Paulo');
ini_set("memory_limit","100M");
header('Content-type: application/json');

require_once('config.php');
include "pretty_json.php";


/*
*
*ERCWS-Desenvolvido por EvaldoRC - 2013
*Moodle v.: 2.4.6 - Open Source
*retorno: json
*
*/

//funcao desejada
$funcao=$_GET['funcao'];

//verifica o token junto ao banco de dados no Moodle
if(!verificaToken($_GET['token']))
{
	echo "Acho que não vai rolar!!  \n\n Acesso não permitido";
}
else
{
	
	switch ($funcao) 
	{
		case 'dados_acesso':
			//id do aluno
			$id= $_GET['id'];
			dados_acesso($id);
			break;
		case 'login':
			//username usuario
			$login= $_GET['login'];
			//senha do usuário
			$passwor=$_POST['passwor'];
			logar($login,$passwor);
			break;
		case 'pegar_cursos':
			//id do aluno
			$id= $_GET['id'];
			pegar_cursos($id);
			break;
		case 'pegar_aulas':
			//id do curso
			$idcurso= $_GET['idcurso'];
			pegar_aulas($idcurso);
			break;
		case 'pegar_arquivos':
			//id do curso
			$idcurso=$_GET['idcurso'];
			//id da aula
			$idaula=$_GET['idaula'];
			pegar_arquivos($idcurso,$idaula);
			break;
		case 'download':
			//id do arquivo vindo do banco de dados
      		$idfile=$_GET['idfile'];
      		//id do aluno que requisita o download
      		$iduser=$_GET['iduser'];
      		download($idfile,$iduser);
      		break;
      	case 'deleta_arquivo':
      		//id do aluno que requisitou o download
          	$iduser= $_GET['iduser'];
          	deletaArquivo($iduser);
          	break;
         case 'pegar_atividades_simples':
         	//id do aluno
         	$iduser=$_GET['iduser'];
         	pegar_atividades_simples($iduser);
         	break;
         case 'pegar_atividades_detalhada':
         	//id do curso
         	$iduser=$_GET['idcurso'];
         	//id da tarefa
         	$idTaref=$_GET['idtarefa'];
         	pegar_atividades_detalhada($iduser,$idTaref);
         	break;
         case 'upload':
         	//id da tarefa
         	$idTaref=$_GET['idtarefa'];
         	//id do usuario
         	$id=$_GET['iduser'];
         	//id do curso
         	$idcurso=$_GET['idcurso'];
         	//tamanho do arquivo
         	$tamanhoFile=$_GET['maxbytes'];
         	upload($idTaref,$id,$idcurso,$tamanhoFile);
         	break;
		default:
			echo  "Funcao nao encontrada";
			break;
	}
				
}

function logar($login, $passwor)
{
		//array que guarda os dados
		$info = array();
		//adiciona junto a criptografia para decodificar a senha
		//password assaltmain 'ver ../moodle/config.php'
		$passwor.='';
		$rs = mysql_query("SELECT u.id,u.firstname,u.lastname,u.username FROM mdl_user u
			 inner join mdl_role_assignments rs on u.id=rs.userid WHERE u.username='$login' AND u.password=MD5('$passwor')
			  AND rs.roleid=5");
		$alunos = array();
		if(mysql_num_rows($rs)>0)
		{
			while($array_user = mysql_fetch_array($rs))
			{
				$alunos["id"]= $array_user['id'];
				$alunos["username"]=$array_user['username'];
				$alunos["firstname"]= $array_user['firstname'];
				$alunos["lastname"]=$array_user['lastname'];
				array_push($info, $alunos);
			}
		}
		//chama a função para montar o JSON e mostra na tela
		print_r(pretty_json(json_encode($info)));
}

function dados_acesso($id)
{
	//array que guarda os dados
	$info = array();
	$rs = mysql_query("SELECT id,username,firstname,lastname FROM mdl_user WHERE id=$id");
	$alunos = array();
	if(mysql_num_rows($rs)>0)
	{
		while($array_user = mysql_fetch_array($rs))
		{
			$alunos["id"]= $array_user['id'];
			$alunos["username"]= $array_user['username'];
			$alunos["firstname"]= $array_user['firstname'];
			$alunos["lastname"]= $array_user['lastname'];
			$alunos["lastname"]=utf8_encode($array_user['lastname']);
			//joga um array dentro do outro
			array_push($info, $alunos);
		}
	}
	//chama a função para montar o JSON e mostra na tela
	print_r(pretty_json(json_encode($info)));
}

function pegar_cursos($id)
{
	//array que guarda os dados
	$info = array();
	$rs = mysql_query("SELECT c.id,c.fullname FROM mdl_role_assignments rs INNER JOIN mdl_context e ON rs.contextid=e.id 
				INNER JOIN  mdl_course c ON c.id = e.instanceid WHERE e.contextlevel=50 AND rs.roleid=5 AND rs.userid=$id
				 and c.visible=1");
			$cursos = array();
			if(mysql_num_rows($rs)>0)
			{
				while($array_course = mysql_fetch_array($rs))
				{
					$cursos["id"]= $array_course['id'];
					$cursos["fullname"]= $array_course['fullname'];
					//codifica para utf-8
					//$cursos["fullname"]=utf8_encode($array_course['fullname']);
					array_push($info, $cursos);
				}
			}
			//chama a função para montar o JSON e mostra na tela
			print_r(pretty_json(json_encode($info)));
}

function pegar_aulas($idcurso)
{
	//array que guarda os dados
	$info = array();
	$rs = mysql_query("select cs.id,cs.summary as name,cs.summary as intro, cs.section as section FROM mdl_course_sections cs
					where cs.course=$idcurso");

			$aulas = array();
			if(mysql_num_rows($rs)>0)
			{

				while($array_aulas = mysql_fetch_array($rs))
				{
					$aulas["id"]= $array_aulas['id'];
					$removeItens=array("<p>","</p>");
					$aulas["name"]= str_replace($removeItens, "", $array_aulas['name']);
					$aulas["intro"]= str_replace($removeItens, "", $array_aulas['intro']);
					$aulas["section"]=$array_aulas['section'];
					if(($aulas["section"]==0)and($aulas["name"]==""))
					{
						$aulas["name"]="Fórum de Notícias";
					}
					//codifica para utf-8
					array_push($info, $aulas);
				}
			}	
			//chama a função para montar o JSON e mostra na tela
			print_r(pretty_json(json_encode($info)));	
}

function pegar_arquivos($idcurso,$idaula)
{
	//array que guarda os dados
	$info = array();
	$rs=mysql_query("select f.id,f.filename,f.contenthash from mdl_files f inner join 
									mdl_context c on f.contextid=c.id inner join
									mdl_course_modules cm on c.instanceid=cm.id
									where cm.course=$idcurso and cm.section=$idaula and f.source !='' 
									and f.component='mod_resource' and cm.visible=1 and cm.module=17");
					$arquivos=array();
					if(mysql_num_rows($rs)>0)
					{
						while($array_arquivos=mysql_fetch_array($rs))
						{
							$arquivos['id']=$array_arquivos['id'];
							$arquivos['filename']=$array_arquivos['filename'];
							$arquivos['contenthash']=$array_arquivos['contenthash'];
							array_push($info,$arquivos);
						}
					}
				//chama a função para montar o JSON e mostra na tela
				print_r(pretty_json(json_encode($info)));
}

function download($fileid,$userid)
{
  //content hash do arquivo
  $conteudo;
  //array que guarda os dados 
  $rs=mysql_query("select contenthash,filename from mdl_files where id=$fileid");
  $arquivo=array();
    if(mysql_num_rows($rs)>0)
    {
      while($arquivo=mysql_fetch_array($rs))
      {
        $conteudo=$arquivo['contenthash'];
        $nomeArquivo=$arquivo['filename'];
	
      }
    }
  $nomeArquivo = iconv('UTF-8', 'ASCII//TRANSLIT', $nomeArquivo);
  $pasta=substr($conteudo,0,2);
  $subpasta=substr($conteudo,2,2);
  
  //caminho da pasta moodledata
  $GLOBALS['caminhoCompleto'] = $GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta;
  
  //caminho da pasta temporária
  $GLOBALS['caminhoTemporario'] = $GLOBALS['caminhoTemporario'].$userid.'/';

  if(file_exists($GLOBALS['caminhoCompleto']))
  {
    
    //dar permissão para a pasta
    chmod($GLOBALS['caminhoCompleto'], 0777);
    //verificar se a pasta existe
    if (file_exists($GLOBALS['caminhoCompleto'].'/'.$conteudo))
    {
      //cria uma pasta com o id do usuário dentro da pasta temporária
      if(mkdir($GLOBALS['caminhoTemporario'],0777))
	{
		
	}	
	if(file_exists($GLOBALS['caminhoTemporario']))
	{
		
	}
	else
	{
		echo 'nao achei a pasta temporaria';
	}
      //copia o arquivo do local original para a pasta temporária
      if(copy($GLOBALS['caminhoCompleto'].'/'.$conteudo, $GLOBALS['caminhoTemporario'].$nomeArquivo))
	{
		//redireciona para o link do arquivo
	       //header("Location: ".$GLOBALS['localServer'].$userid.'/'.$nomeArquivo);	
		header("Location: ".$GLOBALS['localServer'].$userid.'/'.$nomeArquivo);
	}
       else
       {
	  echo 'nao copiou para '.$GLOBALS['caminhoTemporario'].$nomeArquivo;
	
       }
      
    }
    else
    {
      echo "Nao achei o arquivo";
    }
  }
  else
  {
    echo "Nao achei a pasta";
  }
}

function deletaArquivo($userid)
{
  //monta a pasta temporária deste usuário
  $dir = $GLOBALS['caminhoTemporario'].$userid.'/';
  //verifica se é uma pasta
  if(is_dir($dir))
  {

    if($handle = opendir($dir))
    {
      while(($file = readdir($handle)) !== false)
      {
        if($file != '.' && $file != '..')
        {
            unlink($dir.$file);
        }
      }
      //deleta a pasta temporária deste usuário
      rmdir($dir);
      echo "Sucess";
    }
  }
  else
  {
    die("Nao achei a pasta deste usuario");
  }
}

function pegar_atividades_simples($id)
{
	//array que guarda os dados
	$info = array();
	//pega os cursos em q o aluno esta matriculado
	$rs = mysql_query("SELECT c.id FROM mdl_role_assignments rs INNER JOIN mdl_context e ON rs.contextid=e.id INNER JOIN
	  mdl_course c ON c.id = e.instanceid WHERE e.contextlevel=50 AND rs.roleid=5 AND rs.userid=$id");
		$cursos = array();
		if(mysql_num_rows($rs)>0)
		{
			while($array_course = mysql_fetch_array($rs))
			{
				array_push($cursos, $array_course['id']);
			}
		}
		$count = count($cursos);
		//se tiver apenas um curso
		//echo $count;
		if($count=='1')
		{
			$sql="SELECT a.id,gi.itemname,gi.courseid,a.duedate,a.cutoffdate,a.allowsubmissionsfromdate,a.intro,c.fullname
			FROM mdl_grade_items gi 
				inner join mdl_assign a on gi.iteminstance=a.id inner join mdl_course c on c.id=a.course 
				WHERE (courseid=$cursos[0]";	
		}
		//se tiver mais de um curso
		else
		{
			$sql="SELECT a.id,gi.itemname,gi.courseid,a.duedate,a.cutoffdate,a.allowsubmissionsfromdate,a.intro,c.fullname
			FROM mdl_grade_items gi inner join 
				mdl_assign a on gi.iteminstance=a.id inner join mdl_course c on c.id=a.course WHERE (gi.courseid=$cursos[0]";
					
			for($i = 1; $i < ($count); $i++)
			{			
				$sql.=" or gi.courseid=".$cursos[$i];
			}
		}
		//filtrar apenas as 'tarefas'  sem postagem em grupos e ordenar por data de postagem

		$sql.=") and itemtype='mod' and a.nosubmissions=0 and gi.itemmodule='assign' and gi.hidden=0 and a.teamsubmission=0 order by a.duedate";

		//echo $sql;
	//pega todas as atividades do aluno
	$rs = mysql_query($sql);
		$atividades = array();
		if(mysql_num_rows($rs)>0)
		{
			while($array_ativ = mysql_fetch_array($rs))
			{
				//ver se a tarefa pode ser postada de acordo com a data

				if((($array_ativ["cutoffdate"]>=time())Or($array_ativ["cutoffdate"]==0))and($array_ativ["allowsubmissionsfromdate"]<=time()))
				{
						$auxIdAti= $array_ativ['id'];
						//consulta se foi realizada
						$sql="select count(sa.id)
							from mdl_assign_submission sa inner join mdl_assign a on sa.assignment = a.id
							where sa.userid=$id and sa.assignment=$auxIdAti";
						$rs2=mysql_query($sql);
						$ss=mysql_fetch_assoc($rs2);
						if($ss['count(sa.id)']>0)
						{	
						}
						else
						{
							$atividades["id"]= $array_ativ['id'];
							$atividades["itemname"]= $array_ativ['itemname'];
							$atividades["courseid"]= $array_ativ['courseid'];
							$atividades["date"]=(date("d-m-Y G:i",$array_ativ['duedate']));
							$atividades["nome_curso"]=$array_ativ['fullname'];
							//ver se a tarefa já está vencida
							if($array_ativ["duedate"]>=time())
							{
								$atividades["prazo"]="Dentro do prazo";	
							}
							else
							{
								$atividades["prazo"]="Fora do prazo";		
							}
							array_push($info, $atividades);
						}
					
				}
			}
			//chama a função para montar o JSON e mostra na tela
			print_r(pretty_json(json_encode($info)));	
		}
}

function pegar_atividades_detalhada($idCurso,$idTarefa)
{
	//array que guarda os dados
	$info = array();
	$rs = mysql_query("SELECT a.id,a.intro,a.duedate,c.fullname,(select apc.value 
						from mdl_assign a inner join mdl_assign_plugin_config apc on 
						a.id = apc.assignment where apc.name='maxsubmissionsizebytes' and 
						apc.assignment=$idTarefa) as maxbytes 
						FROM mdl_grade_items gi inner join 
						mdl_assign a on gi.iteminstance=a.id inner join 
						mdl_course c on gi.courseid=c.id 
						WHERE (a.id=$idTarefa) group by a.id");
		$atividades = array();
		if(mysql_num_rows($rs)>0)
		{
			while($array_ativ = mysql_fetch_array($rs))
			{
				$atividades["id"]= $array_ativ['id'];
				$atividades["intro"]= $array_ativ['intro'];
				$atividades["duedate"]=(date("d-m-Y G:i",$array_ativ['duedate']));
				$atividades["fullname"]= $array_ativ['fullname'];
				if($array_ativ['maxbytes']==0)
				{
					//verificar o tamanho máximo de envio de arquivo do curso
					$sql2="SELECT maxbytes from mdl_course 
							WHERE id=$idCurso";
					$resultado=mysql_query($sql2);
					while ($row=mysql_fetch_array($resultado)) 
					{
						$atividades["maxbytes"] = $row['maxbytes'];
					}
				}
				else
				{
					$atividades["maxbytes"]= $array_ativ['maxbytes'];;
				}
				array_push($info, $atividades);
			}
			//chama a função para montar o JSON e mostra na tela
			print_r(pretty_json(json_encode($info)));	
		}
}

function upload($idTarefa,$idUser,$idCurso,$maxbytes)
{
	//verifica se a pasta existe
	if((file_exists($GLOBALS['caminhoCompleto'])))
	{
		
	  	//move o arquivo para a pasta
		move_uploaded_file($_FILES["file"]["tmp_name"],$GLOBALS['caminhoCompleto'].
		$_FILES["file"]["name"]);	

		$conteudoHash=sha1_file($GLOBALS['caminhoCompleto'].$_FILES["file"]["name"]);
 		//identifica o nome da pasta e subpasta
 		$pasta=substr($conteudoHash,0,2);
		$subpasta=substr($conteudoHash,2,2);
		//verifica se o arquivo já existe
		if(!file_exists($GLOBALS['caminhoCompleto'].$pasta))
		{
			if(!file_exists($GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta))
			{
				if(!file_exists($GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta.'/'.$conteudoHash))
				{
					//cria as pastas
	  			mkdir($GLOBALS['caminhoCompleto'].$pasta);
	  			mkdir($GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta);
	  			//dá permissoes as pastas
	  			chmod($GLOBALS['caminhoCompleto'], 0777);
	  			chmod($GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta, 0777);
  				//copia o arquivo para a nova pasta
  				copy($GLOBALS['caminhoCompleto']. $_FILES["file"]["name"], $GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta.'/'. $conteudoHash);			
				}
				
			}
		}
  		$time=time();
  		//insert na tabela mdl_assign_submission
  		$sql="INSERT INTO mdl_assign_submission (assignment,userid,timecreated,timemodified,status,groupid) VALUES ($idTarefa,$idUser,$time,$time,'submitted',0)";
  		//echo $sql;
 		mysql_query($sql);
 		
  		$lastid=mysql_insert_id();
 		//insert na tabela mdl_assignsubmission_file
 		$sql= "INSERT INTO mdl_assignsubmission_file (assignment,submission,numfiles) VALUES ($idTarefa,$lastid,1)";
 		//echo $sql;
 		mysql_query($sql);

 		//consultas necessárias para se obter os dados
 		$sql="SELECT id FROM mdl_course_modules WHERE course=$idCurso AND module=1 AND instance=$idTarefa";
 		//echo $sql;
 		$resultado=mysql_query($sql);
 		if(mysql_num_rows($resultado)>0)
		{
			while($array_ativ = mysql_fetch_array($resultado))
			{
				$instanceid=$array_ativ['id'];
			}
		}

		$sql="SELECT id FROM mdl_context WHERE contextlevel=70 AND instanceid=$instanceid";
		//echo $sql;
		$resultado=mysql_query($sql);
		if(mysql_num_rows($resultado)>0)
		{
			while($array=mysql_fetch_array($resultado))
			{
				$contextid=$array['id'];
			}
		}

		$filename=$_FILES["file"]["name"];
 		//insert na tabela mdl_files
		$pathnamehash=sha1('/'.$contextid.'/assignsubmission_file/submission_files/'.$lastid.'/'.$filename);
		$filesize=filesize($GLOBALS['caminhoCompleto'].$pasta.'/'.$subpasta.'/'. $conteudoHash);
		
		$sql="INSERT INTO mdl_files (contenthash,pathnamehash,contextid,component,filearea,itemid,filepath,filename,userid,filesize,mimetype,status,source,author,license,timecreated,timemodified,sortorder,referencefileid,referencelastsync,referencelifetime) VALUES ('$conteudoHash','$pathnamehash',$contextid,'assignsubmission_file','submission_files',$lastid,'/','$filename',$idUser,$filesize,NULL,0,'$filename',NULL,'allrightsreserved',$time,$time,0,NULL,NULL,NULL)";
		//echo $sql;
		mysql_query($sql);

		//deleta o arquivo da pasta primária
  		unlink($GLOBALS['caminhoCompleto'].$_FILES["file"]["name"]);
	}
	else
	{
		echo 'arquivo nao encontrado';
	}
}
?>


