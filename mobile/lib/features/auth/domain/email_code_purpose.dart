enum EmailCodePurpose {
  verifyEmail('verifyEmail'),
  resetPassword('resetPassword');

  const EmailCodePurpose(this.apiValue);

  final String apiValue;
}
