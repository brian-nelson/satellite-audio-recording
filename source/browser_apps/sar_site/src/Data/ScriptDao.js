import BaseDao from "./BaseDao.js"

export default class ScriptDao extends BaseDao {

  getNextLinesByCharacter(projectId, characterId, start, limit) {
    return this.read(`/projects/${projectId}/script/${characterId}/next/${start}/limit/${limit}`);
  }

  getPreviousLinesByCharacter(projectId, characterId, start, limit) {
    return this.read(`/projects/${projectId}/script/${characterId}/previous/${start}/limit/${limit}`);
  }

  getDialogContext(projectId, dialogId) {
    return this.read(`/projects/${projectId}/script/${dialogId}/context`);
  }

  getSceneLines(projectId, sceneId) {
    return this.read(`/projects/${projectId}/script/scene/${sceneId}`);
  }
}