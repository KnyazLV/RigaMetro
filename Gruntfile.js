module.exports = function(grunt) {
  grunt.loadNpmTasks('grunt-contrib-copy');

  grunt.initConfig({
    copy: {
      // Bootstrap
      bootstrap: {
        expand: true,
        cwd: 'node_modules/bootstrap/dist',
        src: ['**/*'],
        dest: 'wwwroot/lib/bootstrap/'
      },
      // jquery-validation
      'jquery-validation': {
        expand: true,
        cwd: 'node_modules/jquery-validation/dist/',
        src: ['jquery.validate.js', 'jquery.validate.min.js'],
        dest: 'wwwroot/lib/jquery-validation/'
      },
      // jquery-validation-unobtrusive
      'jquery-validation-unobtrusive': {
        expand: true,
        cwd: 'node_modules/jquery-validation-unobtrusive/dist/',
        src: ['jquery.validate.unobtrusive.js', 'jquery.validate.unobtrusive.min.js'],
        dest: 'wwwroot/lib/jquery-validation-unobtrusive/'
      }
    }
  });
  
  grunt.registerTask('default', ['copy:bootstrap', 'copy:jquery-validation', 'copy:jquery-validation-unobtrusive']);
};
