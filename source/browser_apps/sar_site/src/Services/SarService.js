import Environment from "../env.js";
import axios from 'axios';
import AuthDao from "../Data/AuthDao";
import SetupDao from "../Data/SetupDao";
import ProjectDao from "../Data/ProjectDao";
import PersonDao from "../Data/PersonDao";
import CharacterDao from "../Data/CharacterDao";
import ScriptDao from "../Data/ScriptDao";

class SarService {
  static JWT = "";
  static UserProperties = null;

  //Projects
  static getProjects() {
    const dao = new ProjectDao(Environment.BASE_URL);
    return dao.getProjects();
  }

  static getProject(id) {
    const dao = new ProjectDao(Environment.BASE_URL);
    return dao.getProject(id);
  }

  static saveProject(proj) {
    const dao = new ProjectDao(Environment.BASE_URL);
    return dao.saveProject(proj);
  }

  static importFountain(id, formData) {
    const dao = new ProjectDao(Environment.BASE_URL);
    return dao.importFountain(id, formData);
  }

  //Participants
  static getParticipantsWithAccess(projectId) {
    const dao = new PersonDao(Environment.BASE_URL);
    return dao.getParticipantsWithAccess(projectId);
  }

  static getParticipant(projectId, personId) {
    const dao = new PersonDao(Environment.BASE_URL);
    return dao.getParticipantWithAccess(projectId, personId);
  }

  static saveParticipantWithAccess(person) {
    const dao = new PersonDao(Environment.BASE_URL);
    return dao.saveParticipantWithAccess(person);
  }

  //Characters
  static getCharactersWithPerformer(projectId) {
    const dao = new CharacterDao(Environment.BASE_URL);
    return dao.getCharactersWithPerformer(projectId);
  }

  static getCharacter(projectId, characterId) {
    const dao = new CharacterDao(Environment.BASE_URL);
    return dao.getCharacter(projectId, characterId);
  }

  static saveCharacter(character) {
    const dao = new CharacterDao(Environment.BASE_URL);
    return dao.saveCharacter(character);
  }

  //Script
  static getNextLinesByCharacter(projectId, characterId, start, length) {
    const dao = new ScriptDao(Environment.BASE_URL);
    return dao.getNextLinesByCharacter(projectId, characterId, start, length);
  }

  static getPreviousLinesByCharacter(projectId, characterId, start, length) {
    const dao = new ScriptDao(Environment.BASE_URL);
    return dao.getPreviousLinesByCharacter(projectId, characterId, start, length);
  }

  static getDialogContext(projectId, dialogId) {
    const dao = new ScriptDao(Environment.BASE_URL);
    return dao.getDialogContext(projectId, dialogId);
  }


  //Authentication and setup
  static login(username, password) {
    const dao = new AuthDao(Environment.BASE_URL);
    return dao.login(username, password);
  }

  static isSetup() {
    const dao = new SetupDao(Environment.BASE_URL);
    return dao.isSetup();
  }

  static setupService(email, password, givenName, familyName, projectName) {
    const dao = new SetupDao(Environment.BASE_URL);
    return dao.setupService(email, password, givenName, familyName, projectName);
  }

  static setJwt(jwt) {
    SarService.JWT = jwt;
    axios.defaults.headers.common['Authorization'] = 'Bearer ' + SarService.JWT;

    this.UserProperties = JSON.parse(atob(jwt.split('.')[1]));
  }

  static setCookie(cname, cvalue, exdays) {
    let d = new Date();
    d.setTime(d.getTime() + (exdays*24*60*60*1000));
    let expires = "expires="+ d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
  }

  static getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for(let i = 0; i <ca.length; i++) {
      let c = ca[i];
      while (c.charAt(0) === ' ') {
        c = c.substring(1);
      }
      if (c.indexOf(name) === 0) {
        return c.substring(name.length, c.length);
      }
    }
    return "";
  }

  static endSession() {
    this.setSession(null);
    this.setJwt("");
  }
}

export default SarService;