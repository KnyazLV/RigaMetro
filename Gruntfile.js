module.exports = function(grunt) {
  grunt.initConfig({
    copy: {
      bootstrap: {
        expand: true,
        cwd: 'node_modules/bootstrap/dist',
        src: ['**/*'],                     
        dest: 'wwwroot/lib/bootstrap/'     
      },
    }
  });

  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.registerTask('default', ['copy']);
};
