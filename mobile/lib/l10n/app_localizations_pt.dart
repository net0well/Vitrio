// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Portuguese (`pt`).
class AppLocalizationsPt extends AppLocalizations {
  AppLocalizationsPt([String locale = 'pt']) : super(locale);

  @override
  String get appTitle => 'Vitrio';

  @override
  String get welcomeTitle => 'Os profissionais da sua cidade, numa vitrine só.';

  @override
  String get welcomeMessage =>
      'Encontre quem atende perto de você, peça orçamento sem compromisso e veja o que os vizinhos acharam.';

  @override
  String get welcomeFindProfessional => 'Encontrar um profissional';

  @override
  String get welcomeOfferServices => 'Quero oferecer meus serviços';

  @override
  String get tabHome => 'Início';

  @override
  String get tabSearch => 'Buscar';

  @override
  String get tabOrders => 'Pedidos';

  @override
  String get tabAccount => 'Conta';

  @override
  String get homeTitle => 'Perto de você';

  @override
  String get homeEmpty =>
      'Logo você vai ver aqui os profissionais bem avaliados da sua cidade.';

  @override
  String get searchTitle => 'Buscar';

  @override
  String get searchEmpty =>
      'Em breve você vai poder buscar por serviço ou pelo nome do profissional.';

  @override
  String get ordersTitle => 'Pedidos';

  @override
  String get ordersEmpty =>
      'Você ainda não fez nenhum pedido. Que tal começar pela busca?';

  @override
  String get ordersEmptyAction => 'Buscar profissionais';

  @override
  String get accountTitle => 'Conta';

  @override
  String get accountEmpty =>
      'Entre ou crie sua conta para pedir orçamentos e oferecer seus serviços.';

  @override
  String get authSignIn => 'Entrar';

  @override
  String get authCreateAccount => 'Criar conta';

  @override
  String get authEmail => 'E-mail';

  @override
  String get authPassword => 'Senha';

  @override
  String get authShowPassword => 'Mostrar senha';

  @override
  String get authHidePassword => 'Esconder senha';

  @override
  String get authForgotPassword => 'Esqueci a senha';

  @override
  String get authName => 'Nome';

  @override
  String get authPasswordRule =>
      'Use pelo menos 10 caracteres. Uma frase com espaços vale.';

  @override
  String get authAcceptTerms =>
      'Li e aceito os termos de uso e a política de privacidade.';

  @override
  String get authAcceptTermsRequired => 'Aceite os termos para continuar.';

  @override
  String get authVerifyTitle => 'Confirme seu e-mail';

  @override
  String authVerifyMessage(String email) {
    return 'Enviamos um código de 6 números para $email. Digite o código para terminar.';
  }

  @override
  String get authCode => 'Código';

  @override
  String get authConfirm => 'Confirmar';

  @override
  String get authResendCode => 'Reenviar código';

  @override
  String authResendIn(int seconds) {
    return 'Reenviar em $seconds s';
  }

  @override
  String get authCodeResent => 'Enviamos um novo código.';

  @override
  String get authChangeEmail => 'Trocar e-mail';

  @override
  String get authForgotTitle => 'Redefinir senha';

  @override
  String get authForgotMessage =>
      'Digite o e-mail da sua conta. Vamos enviar um código para você criar uma senha nova.';

  @override
  String get authSendCode => 'Enviar código';

  @override
  String get authCodeSentIfExists =>
      'Se houver uma conta com este e-mail, enviamos um código para ele.';

  @override
  String get authNewPassword => 'Senha nova';

  @override
  String get authSaveNewPassword => 'Salvar senha nova';

  @override
  String get errorConnection =>
      'Sem conexão. Confira a internet e tente de novo.';

  @override
  String get errorGeneric => 'Algo deu errado. Tente de novo.';

  @override
  String get commonRetry => 'Tentar de novo';

  @override
  String get commonCancel => 'Cancelar';

  @override
  String get commonSave => 'Salvar';

  @override
  String get accountEditProfile => 'Editar dados';

  @override
  String get accountChangePassword => 'Trocar senha';

  @override
  String get accountDevices => 'Aparelhos conectados';

  @override
  String get accountSignOut => 'Sair';

  @override
  String get accountDelete => 'Excluir conta';

  @override
  String get accountDeleteTitle => 'Excluir sua conta?';

  @override
  String get accountDeleteMessage =>
      'Seus dados pessoais serão apagados e você sai de todos os aparelhos. Isso não pode ser desfeito. Para confirmar, digite EXCLUIR.';

  @override
  String get accountDeleteWord => 'EXCLUIR';

  @override
  String get accountDeleteConfirm => 'Excluir';

  @override
  String get profilePhone => 'Celular';

  @override
  String get profilePhoneHint => '(16) 99123-4567';

  @override
  String get profileSaved => 'Dados salvos.';

  @override
  String get passwordCurrent => 'Senha atual';

  @override
  String get passwordChanged =>
      'Senha trocada. Os outros aparelhos foram desconectados.';

  @override
  String get devicesThisDevice => 'Este aparelho';

  @override
  String devicesLastSeen(String date) {
    return 'Último acesso em $date';
  }

  @override
  String get devicesDisconnect => 'Desconectar';

  @override
  String get devicesSignOutAll => 'Sair de todos os aparelhos';

  @override
  String get devicesOnlyThis => 'Só este aparelho está conectado.';
}
